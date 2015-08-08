// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;

namespace Ballistics
{
    // A set of static helper functions that solve for the trajectory that passes through
    // a given target position while also meeting an additional constraint. All spatial
    // parameters are expected to be in principal space. Use ProjectileSpace to convert
    // if necessary. All found solutions are returned as the time at which the target 
    // position must be hit to meet the constraints. Use the function
    // GetInitialVelocityGivenRelativeTargetAndTime() defined in PrincipalProjectile and
    // in Projectile3D to get the initial velocity for the trajectory from this in
    // principal space or in world space, respectively. 
    public abstract class PrincipalTimePlanners
    {
        #region public wrappers

        // Get the time at which the (principal-space) target position r will be hit
        // as part of a trajectory that also passes through position q.
        // Uses Equation 23 from the paper.
        public static float GetTimeToTargetRGivenIntermediatePositionQ(
            PrincipalProjectile projectile, Vector2 r, Vector2 q)
        {
            return GetTimeToTargetRGivenIntermediatePositionQ(
                projectile.k, projectile.vInfinity, r.x, r.y, q.x, q.y);
        }

        // Get the time at which the (principal-space) target position r will be hit 
        // as part of a trajectory that also hits the line exactly 'b' above the 
        // line from the origin to r. This is equivalent to saying that the trajectory 
        // will touch the line y(x) = r.y/r.x * x + b, and that it is exactly contained 
        // by the parallelogram formed by the vectors (0, 0)-(r.x, r.y) and (0, 0)-(0, b)
        // Uses Equation 27 from the paper.
        public static float GetTimeToTargetRGivenArcHeightB(
            PrincipalProjectile projectile, Vector2 r, float b)
        {
            return GetTimeToTargetRGivenArcHeightB(
                projectile.k, projectile.vInfinity, r.x, r.y, b);
        }

        // Get the time at which the (principal-space) target position r will be hit 
        // as part of a trajectory with curviness 'h'. h is defined as the ratio between
        // the height of the parallelogram (as defined in 
        // GetTimeToTargetRGivenArcHeightB()) and the distance to the target. 
        // Consequently, small values result in low arcs, and high values result in high
        // arcs. h = 0.25 roughly approximates a 'minimal effort' arc.
        // Uses Equation 27 and 28 from the paper.
        public static float GetTimeToTargetRGivenCurvinessH(
            PrincipalProjectile projectile, Vector2 r, float h)
        {
            return GetTimeToTargetRGivenCurvinessH(
                projectile.k, projectile.vInfinity, r.x, r.y, h);
        }

        // Get the time at which the (principal-space) target position r will be hit 
        // as part of a trajectory that touches the line y(x) = a*x + b. Use 
        // principalSpace3D.ToPrincipalIntersectionLine() to get the (a,b) line 
        // parameters from a world space plane. Uses Equation 24 from the paper.
        public static float GetTimeToTargetRGivenLineToTouch(
            PrincipalProjectile projectile, Vector2 r, Vector2 lineAB)
        {
            return GetTimeToTargetRGivenLineToTouch(
                projectile.k, projectile.vInfinity, r.x, r.y, lineAB.x, lineAB.y);
        }

        // Get the time at which the (principal-space) target position r will be hit 
        // as part of a trajectory that has slope a at launch (i.e. t = 0). Use
        // principalSpace3D.ToPrincipalSlope() to get the principal slope a from 
        // a slope in world space. Uses Equation 25 from the paper.
        public static float GetTimeToTargetRGivenInitialSlopeA(
            PrincipalProjectile projectile, Vector2 r, float a)
        {
            return GetTimeToTargetRGivenInitialSlopeA(
                projectile.k, projectile.vInfinity, r.x, r.y, a);
        }

        // Get the time at which the (principal-space) target position r will be hit 
        // as part of a trajectory that has slope a at position r. Use
        // principalSpace3D.ToPrincipalSlope() to get the principal slope a from 
        // a slope in world space. Uses Equation 26 from the paper.
        public static float GetTimeToTargetRGivenTargetSlopeA(
            PrincipalProjectile projectile, Vector2 r, float a)
        {
            return GetTimeToTargetRGivenTargetSlopeA(
                projectile.k, projectile.vInfinity, r.x, r.y, a);
        }
        
        // Get the time at which the (principal-space) target position r will be hit 
        // as part of a trajectory with the smallest possible initial velocity.
        // Uses Equation 29 from the paper.
        public static float GetTimeToTargetRWithMinimalInitialSpeed(
            PrincipalProjectile projectile, Vector2 r)
        {
            return (float)GetTimeToTargetRWithMinimalInitialSpeed(
                projectile.k, projectile.vInfinity, r.x, r.y);
        }

        // Get the time at which the (principal-space) target position r will be hit 
        // as part of a trajectory with the initial velocity s. When s is too small 
        // to hit r at all, the functions return 0. But when it great enough, the 
        // problem has two solutions: One arc that's higher and one arc that's lower
        // than the one that's found by GetTimeToTargetRWithMinimalInitialSpeed(). 
        // Use highArc to select which one to return the time for.
        // Uses Equation 30 from the paper.
        public static float GetTimeToTargetRGivenInitialSpeedS(
            PrincipalProjectile projectile, Vector2 r, double s, bool highArc)
        {
            return (float)GetTimeToTargetRGivenInitialSpeedS(
                projectile.k, projectile.vInfinity, r.x, r.y, s, highArc);
        }

        #endregion public wrappers

        #region private implementations

        // Implements Equation 23 from the paper.
        private static float GetTimeToTargetRGivenIntermediatePositionQ(
            float k, float vInfinity, float rX, float rY, float qX, float qY)
        {
            float s = (rX * qY - rY * qX) / (2 * vInfinity * qX);
            float squared = s * (s + 2 * rX / (k * (rX - qX)));
            float t = s + Mathf.Sqrt(squared);
            return t > 0 ? t : 0;
        }

        // Implements Equation 27 from the paper.
        private static float GetTimeToTargetRGivenArcHeightB(
            float k, float vInfinity, float rX, float rY, float b)
        {
            float t = b / vInfinity + 2 * Mathf.Sqrt(b / (k * vInfinity));
            return t > 0 ? t : 0;
        }

        // Implements Equation 28 and uses Equation 27 from the paper.
        private static float GetTimeToTargetRGivenCurvinessH(
            float k, float vInfinity, float rX, float rY, float h)
        {
            float b = h * Mathf.Sqrt(rX * rX + rY * rY);
            return GetTimeToTargetRGivenArcHeightB(k, vInfinity, rX, rY, b);
        }

        // Implements Equation 24 from the paper.
        private static float GetTimeToTargetRGivenLineToTouch(
            float k, float vInfinity, float rX, float rY, float a, float b)
        {
            float kVInfinity = k * vInfinity, aRxMinusRy = a * rX - rY;
            float s = Mathf.Sqrt(b * kVInfinity) + 0.5f * k * (aRxMinusRy + b);
            float t = (s + Mathf.Sqrt(s * s + kVInfinity * aRxMinusRy)) / kVInfinity;
            return t > 0 ? t : 0;
        }

        // Implements Equation 25 from the paper.
        private static float GetTimeToTargetRGivenInitialSlopeA(
            float k, float vInfinity, float rX, float rY, float a)
        {
            float s = 0.5f * k * (a * rX - rY);
            float t = (s + Mathf.Sqrt(s * (s + vInfinity + vInfinity))) / (k * vInfinity);
            return t > 0 ? t : 0;
        }

        // Implements Equation 26 from the paper.
        private static float GetTimeToTargetRGivenTargetSlopeA(
            float k, float vInfinity, float rX, float rY, float a)
        {
            float t = Mathf.Sqrt((rY - a * rX) / (k * vInfinity));
            return t > 0 ? t : 0;
        }

        // Solves Equation 29 from the paper.
        private static double GetTimeToTargetRWithMinimalInitialSpeed(
            double k, double vInfinity, double rX, double rY)
        {
            //   1. Start by getting coefficients for the function f(t) = a4*t^4 + a3*t^3
            //    + a1*t + a0 which is 0 at the sought time-to-target t. Solving f(t) = 0
            //   for t > 0 is equivalent to solving e(u) = f(1/u)*u^4 = a0*u^4 + a1*u^3 +
            //  a3*u + a4 = 0 for u where u = 1 / t, but the latter is more well-behaved,
            //   being a strictly concave function for u > 0 for any set of valid inputs,
            //    so solve e(u)=0 for u instead by converging from an upper bound towards
            double kVInfinity = k * vInfinity, rr = rX * rX + rY * rY;  //   the root and
            double a0 = -rr, a1 = a0 * k, a3 = k * kVInfinity * rY; //        return 1/u.
            double a4 = kVInfinity * kVInfinity;
            double maxInvRelError = 1.0E6; //      Use an achievable inverse error bound.
            double de, e, uDelta = 0;

            //    2. Set u to an upper bound by solving e(u) with a3 = a1 = 0, clamped by
            //            the result of a Newton method's iteration at u = 0 if positive.
            double u = Mathf.Sqrt((float)kVInfinity / Mathf.Sqrt((float)rr));
            if (rY < 0) u = Mathf.Min((float)u, (float)(-vInfinity / rY));

            //   3. Let u monotonically converge to e(u)'s positive root using a modified
            // Newton's method that speeds up convergence for double roots, but is likely
            //             to overshoot eventually. Here, 'e' = e(u) and 'de' = de(u)/du.
            for (int it = 0; it < 10; ++it, uDelta = e / de, u -= 1.9 * uDelta)
            {
                de = a0 * u; e = de + a1; de = de + e; e = e * u;
                de = de * u + e; e = e * u + a3; de = de * u + e; e = e * u + a4;
                if (!(e < 0 && de < 0)) break; //                      Overshot the root.
            }
            u += 0.9 * uDelta; //    Trace back to the unmodified Newton method's output.

            //  4. Continue to converge monotonically from the overestimation u to e(u)'s
            //                                  only positive root using Newton's method.
            for (int it = 0; uDelta * maxInvRelError > u && it < 10; ++it)
            {
                de = a0 * u; e = de + a1; de = de + e; e = e * u;
                de = de * u + e; e = e * u + a3; de = de * u + e; e = e * u + a4;
                uDelta = e / de; u -= uDelta;
            }

            //   5. Return the solved time t to hit [rX, rY], or 0 if no solution exists.
            return (float)(u > 0 ? 1 / u : 0);
        }

        // Solves Equation 30 from the paper.
        private static double GetTimeToTargetRGivenInitialSpeedS(
            double k, double vInfinity, double rX, double rY, double s, bool highArc)
        {
            //   1. Start by getting coefficients for the function f(t) = a4*t^4 + a3*t^3
            //    + a2*t^2 + a1*t + a0 which is 0 at the sought time-to-target t. Solving
            //   f(t) = 0 for t > 0 is equivalent to solving e(u) = f(1/u)*u^3 = a0*u^3 +
            //    a1*u^2 + a2*u + a3 + a4/u for u where u = 1 / t, but the latter is more
            //    well-behaved, being a strictly convex function for u > 0 for any set of
            //  inputs iff a solution exists, so solve for e(u) = 0 instead by converging
            //          from a high or low bound towards the closest root and return 1/u.
            double kRX = k * rX, kRY = k * rY, kRXSq = kRX * kRX, sS = s * s;
            double twoKVInfinityRY = vInfinity * (kRY + kRY), kVInfinity = k * vInfinity;
            double a0 = rX * rX + rY * rY, a1 = (k + k) * a0;
            double a2 = kRXSq + kRY * kRY + twoKVInfinityRY - sS;
            double a3 = twoKVInfinityRY * k, a4 = kVInfinity * kVInfinity;
            double maxInvRelError = 1.0E6; //      Use an achievable inverse error bound.
            double maxV0YSq = sS - kRXSq;//maxV0YSq is the max squared 'V0.y' that leaves
            double e, de, u, uDelta = 0;  //      enough 'V0.x' to reach rX horizontally.

            //        2. Set u to a lower/upper bound for the high/low arc, respectively.
            if (highArc)
            { //  Get smallest u vertically moving rY at max possible +v0.y.
                double minusB = Mathf.Sqrt((float)maxV0YSq) - kRY;
                double determ = minusB * minusB - (twoKVInfinityRY + twoKVInfinityRY);
                u = (kVInfinity + kVInfinity) / (minusB + Mathf.Sqrt((float)determ));
                maxInvRelError = -maxInvRelError; //    Convergence over negative slopes.
            }
            else if (rY < 0)
            { // Get largest u vertically moving rY at most neg. v0.y.
                double minusB = -Mathf.Sqrt((float)maxV0YSq) - kRY;
                double determ = minusB * minusB - (twoKVInfinityRY + twoKVInfinityRY);
                u = (minusB - Mathf.Sqrt((float)determ)) / (rY + rY);
                //   Clamp the above bound by the largest u that reaches rX horizontally.
                u = Mathf.Min((float)(s / rX - k), (float)u);
            }
            else u = s / Mathf.Sqrt((float)a0) - k; //     Get the (largest) u hitting rX
            //         horizontally a.s.a.p. while launching in the direction of [rX,rY].

            //    3. Let u monotonically converge to e(u)'s closest root using a modified
            //   Newton's method, almost scaling the delta as if the solution is a double
            int it = 0; //    root. Note that 'e' = e(u) * u^2 and 'de' = de(u)/du * u^2.
            for (; it < 12; ++it, uDelta = e / de, u -= 1.9 * uDelta)
            {
                de = a0 * u; e = de + a1; de = de + e; e = e * u + a2; de = de * u + e;
                e = e * u + a3; e = (e * u + a4) * u; de = de * u * u - a4;
                if (!(u > 0 && de * maxInvRelError > 0 && e > 0)) break; //     Overshot.
            }
            u += 0.9 * uDelta; //        Trace back to unmodified Newton method's output.

            //         4. Continue to converge monotonically to e(u)'s closest root using
            //    Newton's method from the last known conservative estimate on the convex
            //      function. (Note that in practice, u will have converged enough in <12
            for (; u > 0 && it < 12; ++it)
            {   // iterations iff a solution does exists.)
                de = a0 * u; e = de + a1; de = de + e; e = e * u + a2; de = de * u + e;
                e = e * u + a3; e = (e * u + a4) * u; de = de * u * u - a4;
                uDelta = e / de; u -= uDelta;
                if (!(de * maxInvRelError > 0)) break; // Wrong side of the convex 'dip'.
                if (uDelta * maxInvRelError < u && u > 0) return 1 / u; //  5a. Found it!
            }

            //      5b. If no solution was found, return 0. This only happens if s (minus
            //     a small epsilon) is too small to have a solution, the target is at the
            return 0; //   origin, or the parameters are so extreme they cause overflows.
        }

        #endregion private implementations
    }
}
