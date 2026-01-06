using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonKey
{
    public enum AxisReference
    {
        LOCAL,
        GLOBAL
    }

    public enum Axis
    {
        X,
        Y,
        Z,
        NONE,
    }

    public enum DirectionAxis
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        FORWARD,
        BACK,
        NONE,
    }

    public enum RectCorner
    {
        UP_RIGHT,
        UP_LEFT,
        BOTTOM_RIGHT,
        BOTTOM_LEFT
    }
}

namespace MonKey.Extensions
{
    public static class TransformExt
    {
        public static void Reset(this Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        public static void CopyLocal(this Transform t, Transform other)
        {
            t.localPosition = other.localPosition;
            t.localRotation = other.localRotation;
            t.localScale = other.localScale;
        }

        public static void CopyLocalPositionRotation(this Transform t, Transform other)
        {
            t.localPosition = other.localPosition;
            t.localRotation = other.localRotation;
        }

        public static List<Transform> GetAllSubTransforms(this Transform t, bool includeSelf = false,
            Func<Transform, bool> condition = null)
        {
            var transforms = new List<Transform>();
            if (includeSelf)
            {
                if (condition == null || condition(t))
                {
                    transforms.Add(t);
                }
            }

            foreach (Transform transform in t)
            {
                transforms.AddRange(transform.GetAllSubTransforms(true, condition));
            }

            return transforms;
        }

        public static List<Transform> GetAllParentTransforms(this Transform t)
        {
            var parents = new List<Transform>();
            var parent = t.parent;
            while (parent)
            {
                parents.Add(parent);
                parent = parent.parent;
            }

            return parents;
        }

        public static List<Transform> GetChildren(this Transform t)
        {
            var children = new List<Transform>();
            foreach (Transform transform in t)
            {
                children.Add(transform);
            }

            return children;
        }

        public static Transform GetLastDirectChild(this Transform t)
        {
            if (t.childCount == 0)
            {
                return null;
            }

            return t.GetChild(t.childCount - 1);
        }

        public static Transform GetLowestChild(this Transform t, bool includeSelf = false)
        {
            while (true)
            {
                if (t.GetLastDirectChild() != null)
                {
                    t = t.GetLastDirectChild();
                    includeSelf = true;
                    continue;
                }

                if (includeSelf)
                {
                    return t;
                }

                return null;
            }
        }

        public static List<Transform> GetAllTransformedOrderUpToDown(Func<Transform, bool> condition = null)
        {
            var allTransformOrdered = new List<Transform>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                if (!SceneManager.GetSceneAt(i).isLoaded)
                {
                    continue;
                }

                foreach (var rootGameObject in SceneManager.GetSceneAt(i).GetRootGameObjects())
                {
                    allTransformOrdered.AddRange(rootGameObject.transform.GetAllSubTransforms(true, condition));
                }
            }

            return allTransformOrdered;
        }

        public static Vector3 AxisToVector(this Transform trans, Axis axis, bool local)
        {
            switch (axis)
            {
                case Axis.NONE:
                    return Vector3.zero;
                case Axis.X:
                    return local ? trans.right : Vector3.right;
                case Axis.Y:
                    return local ? trans.up : Vector3.up;
                case Axis.Z:
                    return local ? trans.forward : Vector3.forward;
                default:
                    throw new ArgumentOutOfRangeException("axis", axis, null);
            }
        }

        public static DirectionAxis NextDirection(DirectionAxis axis)
        {
            switch (axis)
            {
                case DirectionAxis.UP:
                    return DirectionAxis.RIGHT;
                case DirectionAxis.DOWN:
                    return DirectionAxis.LEFT;
                case DirectionAxis.LEFT:
                    return DirectionAxis.BACK;
                case DirectionAxis.RIGHT:
                    return DirectionAxis.FORWARD;
                case DirectionAxis.FORWARD:
                    return DirectionAxis.UP;
                case DirectionAxis.BACK:
                    return DirectionAxis.DOWN;
                case DirectionAxis.NONE:
                    return DirectionAxis.NONE;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

        public static Vector3 GlobalAxisToVector(DirectionAxis axis)
        {
            switch (axis)
            {
                case DirectionAxis.RIGHT:
                    return Vector3.right;
                case DirectionAxis.LEFT:
                    return -Vector3.right;
                case DirectionAxis.UP:
                    return Vector3.up;
                case DirectionAxis.DOWN:
                    return -Vector3.up;
                case DirectionAxis.FORWARD:
                    return Vector3.forward;
                case DirectionAxis.BACK:
                    return -Vector3.forward;
                default:
                    return Vector3.zero;
            }
        }

        public static Vector3 AxisToVector(this Transform trans, DirectionAxis axis, bool local)
        {
            switch (axis)
            {
                case DirectionAxis.RIGHT:
                    return local ? trans.right : Vector3.right;
                case DirectionAxis.LEFT:
                    return local ? -trans.right : -Vector3.right;
                case DirectionAxis.UP:
                    return local ? trans.up : Vector3.up;
                case DirectionAxis.DOWN:
                    return local ? -trans.up : -Vector3.up;
                case DirectionAxis.FORWARD:
                    return local ? trans.forward : Vector3.forward;
                case DirectionAxis.BACK:
                    return local ? -trans.forward : -Vector3.forward;
                default:
                    return Vector3.zero;
            }
        }

        public static void AlignTransformToCollision(this Transform trans,
            Vector3 collisionPoint, Vector3 collisionNormal,
            bool alignToNormal = true, DirectionAxis axis = DirectionAxis.UP,
            DirectionAxis globalForward = DirectionAxis.FORWARD)
        {
            trans.position = collisionPoint;
            if (alignToNormal && axis != DirectionAxis.NONE)
            {
                //  trans.rotation = Quaternion.LookRotation(GlobalAxisToVector(globalForward), collisionNormal);
                trans.rotation = Quaternion.FromToRotation(trans.AxisToVector(axis, true),
                    collisionNormal) * trans.rotation;

                var projectedGlobal = Vector3.ProjectOnPlane(GlobalAxisToVector(globalForward), collisionNormal);

                if (Mathf.Approximately(projectedGlobal.magnitude, 0))
                {
                    projectedGlobal = Vector3.ProjectOnPlane(
                        GlobalAxisToVector(NextDirection(globalForward)), collisionNormal);
                }

                var projectedSelf = Vector3.ProjectOnPlane(trans.AxisToVector(globalForward, true), collisionNormal);

                trans.rotation = Quaternion.FromToRotation(projectedSelf, projectedGlobal)
                                 /*Quaternion.AngleAxis(Vector3.Angle(projectedGlobal, projectedSelf), collisionNormal)*/
                                 * trans.rotation;
            }
        }

        public static void SetLossyGlobalScale(this Transform transform, Vector3 globalScale)
        {
            transform.localScale = Vector3.one;
            transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x,
                globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
        }

        public static float InverseLerpLocalPosition(this Transform t, Vector3 localA, Vector3 localB)
        {
            var ab = localB - localA;
            var av = t.localPosition - localA;

            var abLengthSq = Vector3.Dot(ab, ab);
            if (abLengthSq == 0f)
                return 0f;

            var projection = Vector3.Dot(av, ab) / abLengthSq;
            return Mathf.Clamp01(projection);
        }

        public static float InverseLerp(this Transform t, Vector3 a, Vector3 b)
        {
            var ab = b - a;
            var av = t.position - a;

            var abLengthSq = Vector3.Dot(ab, ab);

            if (abLengthSq == 0f)
                return 0f;

            var projection = Vector3.Dot(av, ab) / abLengthSq;
            return Mathf.Clamp01(projection);
        }
    }
}