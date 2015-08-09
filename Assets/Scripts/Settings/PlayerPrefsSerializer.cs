using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

/// <summary>
/// Saves and loads objects in the PlayerPrefs.
/// </summary>
/// Members that are not saved in PlayerPrefs will be ignored during load.
/// The class T may implement IDeserializationCallback if it needs to be notified after deseralization.
class PlayerPrefsSerializer<T> where T : class
{
    private PlayerPrefsSerializerImpl Serializer = new PlayerPrefsSerializerImpl(typeof(T), null);

    private PlayerPrefsSerializer()
    { }

    public static PlayerPrefsSerializer<T> Create()
    {
        return new PlayerPrefsSerializer<T>();
    }

    public void Save(T settings)
    {
        Serializer.Save(settings);
    }

    public T Load()
    {
        return Serializer.Load() as T;
    }

    public void Load(T settings)
    {
        Serializer.Load(settings);
    }

    /// <summary>
    /// Deletes all settings for the type.
    /// </summary>
    public void Clear()
    {
        Serializer.Clear();
    }


    private class PlayerPrefsSerializerImpl
    {
        private Type Type;
        private string KeyPrefix;

        public PlayerPrefsSerializerImpl(Type type, string keyPrefix)
        {
            Type = type;
            KeyPrefix = (keyPrefix ?? type.Name) + ".";
        }

        public void Save(object settings)
        {
            var members = Type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
            foreach (var member in members)
                SaveMember(settings, member);
        }

        public object Load()
        {
            var settings = Activator.CreateInstance(Type);
            Load(settings);
            return settings;
        }

        public void Load(object settings)
        {
            var members = Type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
            foreach (var member in members)
                LoadMember(settings, member);
            TryCallDeserializationCallback(settings);
        }

        public void Clear()
        {
            var members = Type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
            foreach (var member in members)
                ClearMember(member);
        }

        private void ClearMember(MemberInfo member)
        {
            var field = member as FieldInfo;
            if (field != null)
                ClearField(field);
            else
            {
                var property = member as PropertyInfo;
                if (property != null)
                    ClearProperty(property);
            }
            var key = GetKey(member);
            if (PlayerPrefs.HasKey(key))
                PlayerPrefs.DeleteKey(key);
        }

        private void ClearField(FieldInfo field)
        {
            if (IsSupportedComposite(field))
            {
                var serializer = new PlayerPrefsSerializerImpl(field.FieldType, KeyPrefix);
                serializer.Clear();
            }
        }

        private void ClearProperty(PropertyInfo property)
        {
            if (IsSupportedComposite(property))
            {
                var serializer = new PlayerPrefsSerializerImpl(property.PropertyType, KeyPrefix);
                serializer.Clear();
            }
        }

        private void SaveMember(object settings, MemberInfo member)
        {
            var field = member as FieldInfo;
            if (field != null)
                SaveField(settings, field);
            else
            {
                var property = member as PropertyInfo;
                if (property != null)
                    SaveProperty(settings, property);
            }
        }

        private void SaveField(object settings, FieldInfo field)
        {
            if (field.FieldType == typeof(Int32) || field.FieldType.IsEnum)
                SaveInt32((int)field.GetValue(settings), field);
            else if (field.FieldType == typeof(float))
                SaveFloat((float)field.GetValue(settings), field);
            else if (field.FieldType == typeof(string))
                SaveString((string)field.GetValue(settings), field);
            else if (!TrySaveComposite(settings, field))
                ThrowTypeNotSupported(field);
        }

        private bool TrySaveComposite(object settings, FieldInfo field)
        {
            if (!IsSupportedComposite(field))
                return false;
            var serializer = new PlayerPrefsSerializerImpl(field.FieldType, KeyPrefix + field.Name);
            serializer.Save(field.GetValue(settings));
            return true;
        }

        private void ThrowTypeNotSupported(MemberInfo member)
        {
            throw new NotSupportedException(String.Format("Type of {0} not supported.", GetKey(member)));
        }

        private void SaveProperty(object settings, PropertyInfo property)
        {
            var index = new object[0];
            if (property.PropertyType == typeof(Int32) || property.PropertyType.IsEnum)
                SaveInt32((int)property.GetValue(settings, index), property);
            else if (property.PropertyType == typeof(float))
                SaveFloat((float)property.GetValue(settings, index), property);
            else if (property.PropertyType == typeof(string))
                SaveString((string)property.GetValue(settings, index), property);
            else if (!TrySaveComposite(settings, property))
                ThrowTypeNotSupported(property);
        }

        private void SaveInt32(int value, MemberInfo member)
        {
            PlayerPrefs.SetInt(GetKey(member), value);
        }

        private void SaveFloat(float value, MemberInfo member)
        {
            PlayerPrefs.SetFloat(GetKey(member), value);
        }

        private void SaveString(string value, MemberInfo member)
        {
            PlayerPrefs.SetString(GetKey(member), value);
        }

        private bool TrySaveComposite(object settings, PropertyInfo property)
        {
            if (!IsSupportedComposite(property))
                return false;
            var serializer = new PlayerPrefsSerializerImpl(property.PropertyType, KeyPrefix + property.Name);
            serializer.Save(property.GetValue(settings, new object[0]));
            return true;
        }

        private string GetKey(MemberInfo member)
        {
            return String.Format("{0}.{1}", KeyPrefix, member.Name);
        }

        private void LoadMember(object settings, MemberInfo member)
        {
            if (!PlayerPrefs.HasKey(GetKey(member)))
                return;
            var field = member as FieldInfo;
            if (field != null)
                LoadField(settings, field);
            else
            {
                var property = member as PropertyInfo;
                if (property != null)
                    LoadProperty(settings, property);
            }
        }

        private void LoadField(object settings, FieldInfo field)
        {
            if (field.FieldType == typeof(Int32) || field.FieldType.IsEnum)
                field.SetValue(settings, LoadInt32(field));
            else if (field.FieldType == typeof(float))
                field.SetValue(settings, LoadFloat(field));
            else if (field.FieldType == typeof(string))
                field.SetValue(settings, LoadString(field));
            else if (!TryLoadComposite(settings, field))
                ThrowTypeNotSupported(field);
        }

        private bool TryLoadComposite(object settings, FieldInfo field)
        {
            if (!IsSupportedComposite(field))
                return false;
            var serializer = new PlayerPrefsSerializerImpl(field.FieldType, KeyPrefix + field.Name);
            object value = serializer.Load();
            field.SetValue(settings, value);
            return true;
        }

        private static bool IsSupportedComposite(FieldInfo field)
        {
            return field.FieldType.IsClass && field.FieldType.GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0;
        }

        private void LoadProperty(object settings, PropertyInfo property)
        {
            var index = new object[0];
            if (property.PropertyType == typeof(Int32) || property.PropertyType.IsEnum)
                property.SetValue(settings, LoadInt32(property), index);
            else if (property.PropertyType == typeof(float))
                property.SetValue(settings, LoadFloat(property), index);
            else if (property.PropertyType == typeof(string))
                property.SetValue(settings, LoadString(property), index);
            else if (!TryLoadComposite(settings, property))
                ThrowTypeNotSupported(property);
        }

        private int LoadInt32(MemberInfo member)
        {
            return PlayerPrefs.GetInt(GetKey(member));
        }

        private float LoadFloat(MemberInfo member)
        {
            return PlayerPrefs.GetFloat(GetKey(member));
        }

        private string LoadString(MemberInfo member)
        {
            return PlayerPrefs.GetString(GetKey(member));
        }

        private bool TryLoadComposite(object settings, PropertyInfo property)
        {
            if (!IsSupportedComposite(property))
                return false;
            var serializer = new PlayerPrefsSerializerImpl(property.PropertyType, KeyPrefix + property.Name);
            object value = serializer.Load();
            property.SetValue(settings, value, new object[0]);
            return true;
        }

        private static bool IsSupportedComposite(PropertyInfo property)
        {
            return property.PropertyType.IsClass && property.PropertyType.GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0;
        }

        private void TryCallDeserializationCallback(object settings)
        {
            var callback = settings as IDeserializationCallback;
            if (callback == null)
                return;
            callback.OnDeserialization(this);
        }
    }
}
