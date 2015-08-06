// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;
using System.Collections.Generic;
using Ballistics;

// This experimental trajectory plan modifier checks an already existing plan from 
// initialPosition to targetPosition (so, a plan generated already by a planner with a lower 
// planNumber) to see if it intersects with any of the gameobjects specified in 'obstacles'.  
// If found, the old plan is overridden with a plan that shoots over these obstacles, if 
// possible. Use marginDistance to specifiy the minimal distance to keep to any of the 
// objects.  Ideally, marginDistance should be the input to a Minkowski sum procedure, 
// but for simplicity and efficiency reasons, it simply changes that scales of the 
// gameobjects so that their bounding boxes are marginDistance larger when tested against
// the (volumeless) trajectory, making it only a rough approximation.
public class TrajectoryAboveMeshesPlanner : TrajectoryPlannerBase
{
    // list of all the objects that should be checked for collision. 
    public GameObject[] obstacles; 

    // the (crude approximation of) minimal distance to the gameobjects
    public float marginDistance;

    public override bool PlanTimeToTarget(Projectile3D projectile3D,
                                          Vector3 initialPosition,
                                          Vector3 targetPosition,
                                          ref float timeToTarget)
    {
        // Allow this planner to only modify an already existing plan, not create a new one.
        if (timeToTarget <= 0) return false;

        // Convert the problem to principal space
        PrincipalProjectile principalProjectile;
        PrincipalSpace3D principalSpace3D = PrincipalSpace3D.Create(projectile3D,
            initialPosition, targetPosition, out principalProjectile);

        Vector2 principalTargetPosition = principalSpace3D.ToPrincipalPosition(targetPosition);

        PrincipalTrajectory principalTrajectory = new PrincipalTrajectory(principalProjectile);
        principalTrajectory.v0 = principalTrajectory.GetInitialVelocityGivenRelativeTargetAndTime(
                principalTargetPosition, timeToTarget);

        // notify helper classes/structs of the trajectory to test
        _trajectoryOverGameObjectHelper.SetupTrajectory(principalSpace3D, principalTrajectory, principalTargetPosition, marginDistance);

        // (Re-)check all the gameobjects until no further intersections are detected, or until
        // an intersecting gameobject is found that can't have a trajectory passing over it.
        int lastIntersectingGameObjectIndex = -1;
        //bool foundAnotherIntersection;
        //do
        {
            //foundAnotherIntersection = false;

            for (int index = 0; index < obstacles.Length; ++index)
            {
                if (index != lastIntersectingGameObjectIndex)
                {
                    float foundTimeToTarget = _trajectoryOverGameObjectHelper.GetTimeToTargetOverGameObject(obstacles[index]);
                    if (foundTimeToTarget >= 0) // found intersection
                    {
                        //foundAnotherIntersection = true;
                        lastIntersectingGameObjectIndex = index;
                        timeToTarget = foundTimeToTarget;
                        if (timeToTarget == 0)
                        {
                            return true; // trajectory over the object is impossible, so give up
                        }

                        // use the new initial velocity in the tested trajecory
                        principalTrajectory.v0 = principalTrajectory.GetInitialVelocityGivenRelativeTargetAndTime(
                                principalTargetPosition, timeToTarget);
                        // notify helper classes/structs of the new trajectory to test
                        _trajectoryOverGameObjectHelper.SetupTrajectory(principalSpace3D, 
                            principalTrajectory, principalTargetPosition, marginDistance);
                    }
                }
            }
        }
        //while (foundAnotherIntersection);

        return lastIntersectingGameObjectIndex >= 0; // true iff the plan changed
    }

    // This helper class can be used (and reused) to do a full test of a trajectory 
    // against a gameobject's mesh, while returning a new trajectory time over the 
    // obstacle if an intersection was found. 
    private class TrajectoryOverGameObjectHelper
    {
        private Vector3[] _principalVertices = null;
        private PrincipalPlaneBoundingBoxIntersectionTester _principalPlaneBoundingBoxIntersectionTester;
        private TrajectoryOverTriangleSoupHelper _triangleOverTriangleSoupHelper;

        private PrincipalSpace3D _principalSpace3D;
        private PrincipalTrajectory _principalTrajectory;
        private Vector2 _principalTargetPosition;
        private float _marginDistance;

        public void SetupTrajectory(PrincipalSpace3D principalSpace3D,
            PrincipalTrajectory principalTrajectory,
            Vector3 principalTargetPosition,
            float marginDistance)
        {
            _principalSpace3D = principalSpace3D;
            _principalTrajectory = principalTrajectory;
            _principalTargetPosition = principalTargetPosition;
            _marginDistance = marginDistance;

            _triangleOverTriangleSoupHelper.SetupTrajectory(principalTrajectory, principalTargetPosition);
        }

        public float GetTimeToTargetOverGameObject(GameObject gameObject)
        {
            Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
            Matrix4x4 localToPrincipalMatrix = new Matrix4x4();

            if (_principalPlaneBoundingBoxIntersectionTester.IntersectsPrincipalRectangle(
                _principalSpace3D,
                _principalTrajectory,
                _principalTargetPosition,
                gameObject.transform.localToWorldMatrix,
                mesh.bounds.center,
                mesh.bounds.extents,
                _marginDistance,
                ref localToPrincipalMatrix))
            {
                if (_principalVertices == null || _principalVertices.Length < mesh.vertices.Length)
                {
                    _principalVertices = new Vector3[mesh.vertices.Length + 1];
                }

                for (int i = 0; i < mesh.vertices.Length; ++i)
                {
                    _principalVertices[i] = localToPrincipalMatrix.MultiplyPoint(mesh.vertices[i]);
                }

                return _triangleOverTriangleSoupHelper.GetTimeToTargetOverTriangleSoup(_principalSpace3D,
                    _principalTrajectory, _principalTargetPosition, _principalVertices, mesh.triangles);
            }
            return -1;
        }

        // This helper class is used to test the bounding box of a gameobject against 
        // the principal rectangle. The principal rectangle is the smallest axis-aligned
        // rectangle on the principal plane that contains the whole trajectory from start
        // to finish. When the bounding box doesn't intersect the rectangle, the 
        // gameobject is guaranteed not to intersect the trajectory either (although the 
        // converse isn't necessarily true), making this a cheap and conservative 
        // 'broad phase' test. As a side effect, the test also returns the matrix that
        // transforms the gameobject local space to principal space, which can be used 
        // to directly transform the gameobject's vertices into principal space for further
        // testing when this test returns false. A non-zero marginDistance can be used
        // to scale this resulting matrix, growing the mesh by this value in size when 
        // transformed its vertices by it. As explained at the top of the file, this 
        // solution isn't perfect as that would require calculating the Minkowski sum of 
        // the actual mesh instead.
        private struct PrincipalPlaneBoundingBoxIntersectionTester
        {
            public bool IntersectsPrincipalRectangle(
                PrincipalSpace3D principalSpace3D,
                PrincipalTrajectory principalTrajectory, 
                Vector2 principalTargetPosition,
                Matrix4x4 localToWorldMatrix, 
                Vector3 boundsCenter, 
                Vector3 boundsExtents,
                float marginDistance, 
                ref Matrix4x4 localToPrincipalMatrix)
            {
                _localToWorldMatrix = localToWorldMatrix;

                Vector3 axisZ = Vector3.Cross(principalSpace3D.xAxis, principalSpace3D.yAxis);

                // Get the non-uniform scale of the matrix based on the bounding box size
                // and marginDistance
                Vector3 scale = GetMarginMatrixScale(boundsExtents, marginDistance);

                // Get the data necessary to get the z component in principal space 
                // of any local vertex (not considering translation yet).
                localToPrincipalMatrix.m20 = DotMatrixColumn0(axisZ) * scale.x;
                localToPrincipalMatrix.m21 = DotMatrixColumn1(axisZ) * scale.y;
                localToPrincipalMatrix.m22 = DotMatrixColumn2(axisZ) * scale.z;

                Vector3 relBoundsCenter = _localToWorldMatrix.MultiplyPoint(boundsCenter) - principalSpace3D.p0;
                float principalBoundsCenterZ = Vector3.Dot(relBoundsCenter, axisZ);

                // Get the bounding box size projected onto the principal z axis
                float extentsZ = Mathf.Abs(localToPrincipalMatrix.m20) * boundsExtents.x +
                                 Mathf.Abs(localToPrincipalMatrix.m21) * boundsExtents.y +
                                 Mathf.Abs(localToPrincipalMatrix.m22) * boundsExtents.z;

                if (Mathf.Abs(principalBoundsCenterZ) > extentsZ)
                {
                    return false; // bounding box doens't intersect the principal plane
                }

                // Get the data necessary to get the x component in principal space 
                // of any local vertex (not considering translation yet).
                localToPrincipalMatrix.m00 = DotMatrixColumn0(principalSpace3D.xAxis) * scale.x;
                localToPrincipalMatrix.m01 = DotMatrixColumn1(principalSpace3D.xAxis) * scale.y;
                localToPrincipalMatrix.m02 = DotMatrixColumn2(principalSpace3D.xAxis) * scale.z;
                
                // Test the bounding box projected on the x axis of the principal space 
                // against the initial (i.e. min) and target (i.e. max) x interval.
                float principalBoundsCenterX = Vector3.Dot(relBoundsCenter, principalSpace3D.xAxis);
                float extentsX = Mathf.Abs(localToPrincipalMatrix.m00) * boundsExtents.x +
                                 Mathf.Abs(localToPrincipalMatrix.m01) * boundsExtents.y +
                                 Mathf.Abs(localToPrincipalMatrix.m02) * boundsExtents.z;
                if (principalBoundsCenterX + extentsX < 0 ||
                    principalBoundsCenterX - extentsX > principalTargetPosition.x)
                {
                    return false;
                }

                // Get the data necessary to get the y component in principal space 
                // of any local vertex (not considering translation yet).
                localToPrincipalMatrix.m10 = DotMatrixColumn0(principalSpace3D.yAxis) * scale.x;
                localToPrincipalMatrix.m11 = DotMatrixColumn1(principalSpace3D.yAxis) * scale.y;
                localToPrincipalMatrix.m12 = DotMatrixColumn2(principalSpace3D.yAxis) * scale.z;

                // Test the bounding box projected on the y axis of the principal space 
                // against the min and max y interval.
                float principalBoundsCenterY = Vector3.Dot(relBoundsCenter, principalSpace3D.yAxis);
                float extentsY = Mathf.Abs(localToPrincipalMatrix.m10) * boundsExtents.x +
                                 Mathf.Abs(localToPrincipalMatrix.m11) * boundsExtents.y +
                                 Mathf.Abs(localToPrincipalMatrix.m12) * boundsExtents.z;
                if (principalBoundsCenterY + extentsY < Mathf.Min(0.0f, principalTargetPosition.y) ||
                    principalBoundsCenterY - extentsY > principalTrajectory.PositionAtTime(
                    principalTrajectory.GetTimeAtMaximumHeight()).y)
                {
                    return false;
                }

                // Get the data necessary to do the translation part from local to
                // principal space
                Vector3 delta = new Vector3(_localToWorldMatrix.m03 - principalSpace3D.p0.x,
                                            _localToWorldMatrix.m13 - principalSpace3D.p0.y,
                                            _localToWorldMatrix.m23 - principalSpace3D.p0.z);
                localToPrincipalMatrix.m03 = Vector3.Dot(delta, principalSpace3D.xAxis);
                localToPrincipalMatrix.m13 = Vector3.Dot(delta, principalSpace3D.yAxis);
                localToPrincipalMatrix.m23 = Vector3.Dot(delta, axisZ);

                // No projection components
                localToPrincipalMatrix.m30 = 0;
                localToPrincipalMatrix.m31 = 0;
                localToPrincipalMatrix.m32 = 0;
                localToPrincipalMatrix.m33 = 1;

                return true;
            }

            private float GetAxisScale(int c)
            {
                float x = _localToWorldMatrix[0, c], y = _localToWorldMatrix[1, c], z = _localToWorldMatrix[2, c];
                return Mathf.Sqrt(x * x + y * y + z * z);
            }

            private Vector3 GetMarginMatrixScale(Vector3 boundsExtents, float marginDistance)
            {
                if (marginDistance != 0)
                {
                    Vector3 marginScale = new Vector3(
                        1.0f / (GetAxisScale(0) * boundsExtents.x),
                        1.0f / (GetAxisScale(1) * boundsExtents.y),
                        1.0f / (GetAxisScale(2) * boundsExtents.z));

                    return Vector3.one + marginScale * marginDistance;
                }
                return Vector3.one;
            }

            private float DotMatrixColumn0(Vector3 v)
            {
                return v.x * _localToWorldMatrix.m00 + v.y * _localToWorldMatrix.m10 + v.z * _localToWorldMatrix.m20;
            }

            private float DotMatrixColumn1(Vector3 v)
            {
                return v.x * _localToWorldMatrix.m01 + v.y * _localToWorldMatrix.m11 + v.z * _localToWorldMatrix.m21;
            }

            private float DotMatrixColumn2(Vector3 v)
            {
                return v.x * _localToWorldMatrix.m02 + v.y * _localToWorldMatrix.m12 + v.z * _localToWorldMatrix.m22;
            }

            private Matrix4x4 _localToWorldMatrix;
        }

        // Test a trajectory against a mesh's triangle soup.
        struct TrajectoryOverTriangleSoupHelper
        {
            private EdgeClassifier _edgeClassifier;
            private EdgesOnPrincipalPlane _edgesOnPrincipalPlane;

            public void SetupTrajectory(PrincipalTrajectory principalTrajectory, Vector2 principalTargetPosition)
            {
                _edgeClassifier.SetupTrajectory(principalTrajectory, principalTargetPosition);
            }

            public float GetTimeToTargetOverTriangleSoup(
                PrincipalSpace3D principalSpace3D, 
                PrincipalTrajectory principalTrajectory,
                Vector2 principalTargetPosition,
                Vector3[] principalVertices,
                int[] indexTriplets)
            {
                int numTriangles = indexTriplets.Length / 3;

                bool foundIntersection = false;
                _edgesOnPrincipalPlane.Initialize(numTriangles);

                for (int index = 0; index < indexTriplets.Length; index += 3)
                {
                    // Get the vertices of the current triangle
                    Vector3 v0 = principalVertices[indexTriplets[index]];
                    Vector3 v1 = principalVertices[indexTriplets[index + 1]];
                    Vector3 v2 = principalVertices[indexTriplets[index + 2]];

                    // Test if triangle intersects the principal plane
                    if (_edgesOnPrincipalPlane.TryAddTriangle(v0, v1, v2))
                    {
                        bool withinXRange;

                        
                        Debug.DrawLine(principalSpace3D.p0 +
                                       principalSpace3D.xAxis * _edgesOnPrincipalPlane.vertices[_edgesOnPrincipalPlane.vertexCount - 2].x +
                                       principalSpace3D.yAxis * _edgesOnPrincipalPlane.vertices[_edgesOnPrincipalPlane.vertexCount - 2].y,
                                       principalSpace3D.p0 +
                                       principalSpace3D.xAxis * _edgesOnPrincipalPlane.vertices[_edgesOnPrincipalPlane.vertexCount - 1].x +
                                       principalSpace3D.yAxis * _edgesOnPrincipalPlane.vertices[_edgesOnPrincipalPlane.vertexCount - 1].y,
                                       _edgeClassifier.IntersectsTrajectory(
                                       _edgesOnPrincipalPlane.vertices[_edgesOnPrincipalPlane.vertexCount - 2],
                                       _edgesOnPrincipalPlane.vertices[_edgesOnPrincipalPlane.vertexCount - 1],
                                      out withinXRange) ? Color.red : ((v1.z - v0.z) * (v2.x - v0.x) - (v1.x - v0.x) * (v2.z - v0.z) > 0 ? Color.blue : Color.green));
                        

                        // Test if the triangle intersects the trajectory
                        if (_edgeClassifier.IntersectsTrajectory(
                            _edgesOnPrincipalPlane.vertices[_edgesOnPrincipalPlane.vertexCount - 2],
                            _edgesOnPrincipalPlane.vertices[_edgesOnPrincipalPlane.vertexCount - 1],
                            out withinXRange))
                        {
                            foundIntersection = true;
                        }

                        // Discard all segments for down-facing triangles and those
                        // segments that are outside the trajectory's valid x range.
                        // What remains is kept around for the case a trajectory over
                        // the obstacles needs to be calculated later on.
                        bool facesDown = (v1.z - v0.z) * (v2.x - v0.x) - (v1.x - v0.x) * (v2.z - v0.z) < 0;
                        if (!withinXRange || facesDown)
                        {
                            _edgesOnPrincipalPlane.RemoveLastAddedEdge();
                        }
                    }
                }

                if (foundIntersection)
                {
                    return GetTimeToTargetOnIntersection(principalSpace3D, principalTrajectory, principalTargetPosition);
                }

                return -1;
            }

            // Find a trajectory for which the trajectory's y is at least as high as any
            // of the vertices remaining in _edgesOnPrincipalPlane.
            private float GetTimeToTargetOnIntersection(
                PrincipalSpace3D principalSpace3D, 
                PrincipalTrajectory principalTrajectory,
                Vector2 principalTargetPosition)
            {

                for (int index = 0; index < _edgesOnPrincipalPlane.vertexCount; index += 2)
                {
                    if (_edgeClassifier.CrossesXBound(
                        _edgesOnPrincipalPlane.vertices[index],
                        _edgesOnPrincipalPlane.vertices[index + 1]))
                    {
                        return 0;
                    }
                }


                float timeToTarget = 0.0f;
                for (int index = 0; index < _edgesOnPrincipalPlane.vertexCount; index += 2)
                {
                    Vector2 vertex = _edgesOnPrincipalPlane.vertices[index];
                    if (principalTrajectory.PositionYAtX(vertex.x) < vertex.y)
                    {
                        /*
                        Debug.DrawLine(principalSpace3D.p0, 
                                       principalSpace3D.p0 + principalSpace3D.xAxis * vertex.x + principalSpace3D.yAxis * vertex.y, 
                                       Color.grey);
                        */
                        timeToTarget = PrincipalTimePlanners.GetTimeToTargetRGivenIntermediatePositionQ(
                                principalTrajectory,
                                principalTargetPosition,
                                vertex);

                        principalTrajectory.v0 =
                            principalTrajectory.GetInitialVelocityGivenRelativeTargetAndTime(
                            principalTargetPosition, timeToTarget);
                    }
                }
                return timeToTarget;
            }
        }

        // Helper class that finds the edge/segment that defines the intersection between
        // the principal plane and a triangle, If such a segment exists, it's two 2D 
        // vertices are added to the back of 'vertices'.
        public struct EdgesOnPrincipalPlane
        {
            public Vector2[] vertices;
            public int vertexCount;

            public void Initialize(int numTriangles)
            {
                int maxVertices = numTriangles * 2 + 1;
                if (vertices == null || vertices.Length < maxVertices)
                {
                    vertices = new Vector2[maxVertices];
                }
                vertexCount = 0;
            }

            public bool TryAddTriangle(Vector3 vertexA,
                                   Vector3 vertexB,
                                   Vector3 vertexC)
            {
                int oldVertexCount = vertexCount;

                TryAddVertex(vertexA, vertexB);
                TryAddVertex(vertexB, vertexC);
                TryAddVertex(vertexC, vertexA);

                if (vertexCount - oldVertexCount < 2)
                {
                    vertexCount = oldVertexCount;
                    return false;
                }
                else if (vertexCount - oldVertexCount > 2)
                {
                    float ab = (vertices[oldVertexCount + 0] - vertices[oldVertexCount + 1]).sqrMagnitude;
                    float bc = (vertices[oldVertexCount + 1] - vertices[oldVertexCount + 2]).sqrMagnitude;
                    float ca = (vertices[oldVertexCount + 2] - vertices[oldVertexCount + 0]).sqrMagnitude;

                    if (bc > ca)
                    {
                        if (bc > ab)
                        {
                            vertices[oldVertexCount + 0] = vertices[oldVertexCount + 2];
                        }
                    }
                    else if (ca > ab)
                    {
                        vertices[oldVertexCount + 1] = vertices[oldVertexCount + 2];
                    }

                    --vertexCount;
                }
                return true;
            }

            public void RemoveLastAddedEdge()
            {
                vertexCount -= 2;
            }

            private void TryAddVertex(Vector3 a, Vector3 b)
            {
                if (a.z * b.z <= 0 && a.z != b.z)
                {
                    float weightB = a.z / (a.z - b.z);
                    vertices[vertexCount] = new Vector2(a.x + (b.x - a.x) * weightB,
                                                        a.y + (b.y - a.y) * weightB);

                    ++vertexCount;
                }
            }
        }

        // Helper class that can do exact intersection tests of a trajectory with 
        // arbitrary edges on the principal plane. In effect, it tests whether or
        // not an x exists within the edge's x interval for which both the line and
        // trajectory have the same y. Note that finding the x values for the
        // intersections can be solved with abc formula. But as it's not required to 
        // calculate the actual exact x roots, but only to test whether or not a root 
        // lies within the x range of the edge, the inequality following from the
        // abc formula can be rewritten using basic algebra to solve the problem 
        // without actually doing a square root.
        public struct EdgeClassifier
        {
            public void SetupTrajectory(PrincipalTrajectory principalTrajectory, Vector2 principalTargetPosition)
            {
                float k = principalTrajectory.k;
                float vInfinity = principalTrajectory.vInfinity;
                float v0x = principalTrajectory.v0.x;
                float v0y = principalTrajectory.v0.y;

                _twoAPerDY = (k + k) * v0x;
                _twoAPerDX = -(k + k) * (v0y + vInfinity);
                _bPerDYAndCPerBiasDX = -v0x * v0x;
                _bPerBiasDX = v0x * k;
                _bPerDX = v0x * v0y;
                _targetPosition = principalTargetPosition;
                _principalTrajectory = principalTrajectory;
            }

            public bool IntersectsTrajectory(Vector2 v0, Vector2 v1, out bool withinXRange)
            {
                withinXRange = false;

                float dx = v1.x - v0.x;
                float dy = v1.y - v0.y;

                float minX, maxX;
                if (v0.x < v1.x) { minX = v0.x; maxX = v1.x; }
                else { minX = v1.x; maxX = v0.x; }

                if (minX < 0) { if (maxX < 0) return false; else minX = 0; }
                if (maxX > _targetPosition.x) { if (minX > _targetPosition.x) return false; else maxX = _targetPosition.x; }

                withinXRange = true;

                float dySq = dy * dy;
                if (dx * dx < dySq * 1E-12f)
                {
                    float aToTrajectory = _principalTrajectory.PositionYAtX(minX) - v0.y;
                    float twoTimesMidToTrajectory = aToTrajectory + aToTrajectory + dy;
                    return twoTimesMidToTrajectory * twoTimesMidToTrajectory <= dySq;
                }

                float biasDX = v0.y * dx - v0.x * dy;
                float twoA = _twoAPerDY * dy + _twoAPerDX * dx;
                float b = _bPerDYAndCPerBiasDX * dy + _bPerBiasDX * biasDX + _bPerDX * dx;
                float c = _bPerDYAndCPerBiasDX * biasDX;

                float det = b * b - (twoA + twoA) * c;
                float z0 = minX * twoA + b, z1 = maxX * twoA + b;
                float z0Sq = z0 * z0, z1Sq = z1 * z1;
                return z0Sq > z1Sq ? det > (z0 * z1 <= 0 ? 0 : z1Sq) && det < z0Sq :
                                     det > (z0 * z1 <= 0 ? 0 : z0Sq) && det < z1Sq;
            }

            public bool CrossesXBound(Vector2 a, Vector2 b)
            {
                bool aMin = a.x < 0;
                bool aMax = a.x > _targetPosition.x;
                bool bMin = b.x < 0;
                bool bMax = b.x > _targetPosition.x;

                if (aMin != bMin)
                {
                    float yAtInitialX = a.y + -a.x * (b.y - a.y) / (b.x - a.x);
                    if (yAtInitialX > 0)
                    {
                        return true;
                    }
                }
                if (aMax != bMax)
                {
                    float yAtTargetX = a.y + (_targetPosition.x - a.x) * (b.y - a.y) / (b.x - a.x);
                    if (yAtTargetX > _targetPosition.y)
                    {
                        return true;
                    }
                }
                return false;
            }

            private float _twoAPerDY, _twoAPerDX, _bPerDYAndCPerBiasDX, _bPerBiasDX, _bPerDX;
            private Vector2 _targetPosition;
            private PrincipalTrajectory _principalTrajectory;
        };
    }

    // The _trajectoryOverGameObjectHelper is mainly made static to be able to reuse the vertex buffer.
    private static TrajectoryOverGameObjectHelper _trajectoryOverGameObjectHelper = new TrajectoryOverGameObjectHelper();
}

