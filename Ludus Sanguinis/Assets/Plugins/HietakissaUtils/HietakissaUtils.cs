namespace HietakissaUtils
{
    using Random = UnityEngine.Random;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Newtonsoft.Json;
    using Serialization;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using System.IO;
    using System;

    public static class Extensions
    {
        #region Vector2 Extensions
        public static Vector2 SetX(this Vector2 vector, float x)
        {
            vector.x = x;
            return vector;
        }
        public static Vector2 SetY(this Vector2 vector, float y)
        {
            vector.y = y;
            return vector;
        }
        public static Vector2 SetXY(this Vector2 vector, float x, float y)
        {
            vector.Set(x, y);
            return vector;
        }
        #endregion
        #region Vector3 Extensions
        public static Vector3 SetX(this Vector3 vector, float value)
        {
            vector.x = value;
            return vector;
        }
        public static Vector3 SetY(this Vector3 vector, float value)
        {
            vector.y = value;
            return vector;
        }
        public static Vector3 SetZ(this Vector3 vector, float value)
        {
            vector.z = value;
            return vector;
        }
        public static Vector3 SetXY(this Vector3 vector, float x, float y)
        {
            vector.Set(x, y, vector.z);
            return vector;
        }
        public static Vector3 SetXZ(this Vector3 vector, float x, float z)
        {
            vector.Set(x, vector.y, z);
            return vector;
        }
        public static Vector3 SetYZ(this Vector3 vector, float y, float z)
        {
            vector.Set(vector.x, y, z);
            return vector;
        }

        public static Vector3 DirectionTo(this Transform t, Vector3 position) => Maf.Direction(t.position, position);
        #endregion
        #region Float Extensions
        public static int RoundUp(this float roundNum) => (int)Math.Ceiling(roundNum);
        public static int RoundDown(this float roundNum) => (int)Math.Floor(roundNum);
        public static int RoundToNearest(this float num) => Maf.RoundToNearest(num);
        public static float RoundToNearest(this float num, float roundBy) => Maf.RoundToNearest(num, roundBy);
        public static float RoundToDecimalPlaces(this float num, int decimalPlaces) => Maf.RoundToDecimalPlaces(num, decimalPlaces);

        public static float Abs(this float absFloat) => Mathf.Abs(absFloat);

        public static float FlipOne(this float num) => Maf.FlipOne(num);
        #endregion
        #region String Extensions
        public static string AddInFrontOfMatches(this string text, string textToAdd, params string[] matches)
        {
            foreach (string match in matches) text = text.Replace(match, $"{textToAdd}{match}");
            return text;
        }
        public static string ReplaceMultiple(this string text, string replacement, params string[] targets)
        {
            foreach (string target in targets) text.Replace(target, replacement);
            return text;
        }
        public static string ReplaceFirst(this string text, string match, string replacement)
        {
            int pos = text.IndexOf(match);

            if (pos < 0) return text;

            return text.Substring(0, pos) + replacement + text.Substring(pos + match.Length);
        }
        public static string Remove(this string targetString, string stringToRemove)
        {
            return targetString.Replace(stringToRemove, "");
        }
        public static string RemoveFirst(this string targetString, string stringToRemove)
        {
            return targetString.ReplaceFirst(stringToRemove, "");
        }
        #endregion

        public static int Abs(this int absInt)
        {
            return Mathf.Abs(absInt);
        }

        public static Quaternion ToQuaternion(this Vector3 euler)
        {
            return Quaternion.Euler(euler);
        }
        public static Vector3 ToEuler(this Quaternion quaternion)
        {
            return quaternion.eulerAngles;
        }

        public static int Normalized(this float num) => num > 0 ? 1 : (num < 0 ? -1 : 0);

        public static TElement RandomElement<TElement>(this TElement[] array) => array[Random.Range(0, array.Length)];
        public static TElement RandomElement<TElement>(this List<TElement> list) => list[Random.Range(0, list.Count)];

        public static bool IndexInBounds<TType>(this TType[] array, int index) => index >= 0 && index < array.Length;
        public static bool IndexInBounds<TType>(this TType[,] array, int x, int y) => x >= 0 && x < array.GetLength(0) && y >= 0 && y < array.GetLength(1);
        public static bool IndexInBounds<TType>(this TType[,,] array, int x, int y, int z) => x >= 0 && x < array.GetLength(0) && y >= 0 && y < array.GetLength(1) && z >= 0 && z < array.GetLength(2);
        public static bool IndexInBounds<TType>(this List<TType> array, int index) => index >= 0 && index < array.Count;

        public static bool Contains<TType>(this TType[] array, TType value)
        {
            int length = array.Length;
            for (int i = 0; i < length; i++)
            {
                if (Equals(array[i], value)) return true;
            }
            return false;
        }
        public static bool Contains(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }

        public static void DestroyChildren(this Transform transform)
        {
            int childCount = transform.childCount;

            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
        }
        public static void DestroyChildrenImmediate(this Transform transform)
        {
            int childCount = transform.childCount;

            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        public static float CalculateVolume(this Mesh mesh)
        {
            const float multiplier = 0.1666666667f;
            float volume = 0;

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            Vector3 p1;
            Vector3 p2;
            Vector3 p3;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                p1 = vertices[triangles[i + 0]];
                p2 = vertices[triangles[i + 1]];
                p3 = vertices[triangles[i + 2]];
                volume += SignedVolumeOfTriangle(p1, p2, p3);
            }
            return Mathf.Abs(volume);

            float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
            {
                float v321 = p3.x * p2.y * p1.z;
                float v231 = p2.x * p3.y * p1.z;
                float v312 = p3.x * p1.y * p2.z;
                float v132 = p1.x * p3.y * p2.z;
                float v213 = p2.x * p1.y * p3.z;
                float v123 = p1.x * p2.y * p3.z;

                return multiplier * (-v321 + v231 + v312 - v132 - v213 + v123);
            }
        }

        public static void GetOrAddComponent<TType>(this Transform transform, out TType component) where TType : Component
        {
            if (!transform.TryGetComponent(out component)) component = transform.gameObject.AddComponent<TType>();
        }
        public static void GetOrAddComponent<TType>(this GameObject gameObject, out TType component) where TType : Component
        {
            if (!gameObject.TryGetComponent(out component)) component = gameObject.AddComponent<TType>();
        }


        public static Texture2D ToTexture2D(this RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
            RenderTexture oldRT = RenderTexture.active;
            RenderTexture.active = rTex;

            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();

            RenderTexture.active = oldRT;
            return tex;
        }

        public static Vector3 RandomPoint(this Bounds bounds) => Maf.RandomPointInBounds(bounds);

        public static Vector3 GetCanvasPositionForElement(this Canvas canvas, RectTransform uiElement) => canvas.transform.InverseTransformPoint(uiElement.position);
    }

    public abstract class Maf
    {
        public static Vector2 Vector2Average(params Vector2[] vectors)
        {
            float totalX = 0f, totalY = 0f;

            for (int i = 0; i < vectors.Length; i++)
            {
                Vector2 vector = vectors[i];

                totalX += vector.x;
                totalY += vector.y;
            }

            return new Vector2(totalX / vectors.Length, totalY / vectors.Length);
        }
        public static Vector3 Vector3Average(params Vector3[] vectors)
        {
            float totalX = 0f, totalY = 0f, totalZ = 0f;

            for (int i = 0; i < vectors.Length; i++)
            {
                Vector3 vector = vectors[i];

                totalX += vector.x;
                totalY += vector.y;
                totalZ += vector.z;
            }

            return new Vector3(totalX / vectors.Length, totalY / vectors.Length, totalZ / vectors.Length);
        }

        public static Vector3 Direction(Vector3 from, Vector3 to) => (to - from).normalized;

        public static float Average(params float[] nums)
        {
            float num = 0f;
            for (int i = 0; i < nums.Length; i++)
            {
                num += nums[i];
            }

            return num / nums.Length;
        }

        public static int RoundToNearest(float num) => (int)Math.Round(num, MidpointRounding.AwayFromZero);
        public static float RoundToNearest(float num, float roundBy)
        {
            float remainder = Mathf.Abs(num % roundBy);

            if (num > 0)
            {
                if (remainder < roundBy * 0.5f) return num - remainder;
                else return num + (roundBy - remainder);
            }
            else
            {
                if (remainder < roundBy * 0.5f) return num + remainder;
                else return num - (roundBy - remainder);
            }
        }
        public static float RoundToDecimalPlaces(float num, int decimalPlaces) => (float)Math.Round((decimal)num, decimalPlaces);

        public static float ReMap(float iMin, float iMax, float oMin, float oMax, float value)
        {
            float t = Mathf.InverseLerp(iMin, iMax, value);
            return Mathf.Lerp(oMin, oMax, t);
        }
        public static float FlipOne(float num)
        {
            return Mathf.Abs(1f - num);
        }

        public static Vector3 QuaternionToEuler(Quaternion quaternion)
        {
            return quaternion.eulerAngles;
        }
        public static Quaternion EulerToQuaternion(Vector3 euler) => Quaternion.Euler(euler);
        public static Quaternion EulerToQuaternion(float x, float y, float z) => Quaternion.Euler(x, y, z);

        public static bool RandomBool(int percentage) => Random.Range(1, 101) <= percentage;
        public static bool RandomBool(float percentage) => Random.Range(0f, 1f) <= percentage * 0.01f;

        public static Quaternion RandomDirection => Quaternion.LookRotation(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)), Vector3.up);

        public static Vector3 RandomPointInBounds(Bounds bounds) => bounds.center + new Vector3(Random.Range(-bounds.size.x * 0.5f, bounds.size.x * 0.5f), Random.Range(-bounds.size.y * 0.5f, bounds.size.y * 0.5f), Random.Range(-bounds.size.z * 0.5f, bounds.size.z * 0.5f));

        public static Vector3 CalculateCollisionPoint(Vector3 targetPos, Vector3 targetVelocity, Vector3 projectilePos, float projectileSpeed)
        {
            float timeToReachObject = CalculateTimeToCollision(targetPos - projectilePos, targetVelocity, projectileSpeed);

            if (timeToReachObject > 0f) return targetPos + targetVelocity * timeToReachObject;
            else return Vector3.zero;

            float CalculateTimeToCollision(Vector3 relativePosition, Vector3 relativeVelocity, float projectileSpeed)
            {
                float rVel = Vector3.Dot(relativeVelocity, relativeVelocity) - projectileSpeed * projectileSpeed;
                float rAvg = Vector3.Dot(relativeVelocity, relativePosition) * 2f;
                float rPos = Vector3.Dot(relativePosition, relativePosition);

                float disc = rAvg * rAvg - rVel * rPos * 4f;

                if (disc > 0f) return rPos * 2f / (Mathf.Sqrt(disc) - rAvg);
                else return -1f;
            }
        }
    }

    public static class ControlRebinding
    {
        static Dictionary<string, KeyCode> bindings;

        static KeyCode[] validKeycodes;
        static KeyCode keyCode;

        public static bool binding { get; private set; }
        static string bindingKeyName;

        public static event Action OnKeyRebound;

        public static void SetValidKeycodes(bool includeController = false)
        {
            bindings = new Dictionary<string, KeyCode>();
            validKeycodes = Enum.GetValues(typeof(KeyCode))
                .Cast<KeyCode>()
                .Where(k => !includeController ? (int)k < 330 : true)
                .ToArray();
        }

        public static KeyCode GetPressedKey()
        {
            if (!Input.anyKeyDown) return KeyCode.None;

            for (int i = 0; i < validKeycodes.Length; i++)
            {
                keyCode = validKeycodes[i];
                if (Input.GetKeyDown(keyCode)) return keyCode;
            }

            return KeyCode.None;
        }

        public static KeyCode GetKeyWithName(string name)
        {
            return bindings[name];
        }

        public static void StartBinding(string name)
        {
            binding = true;
            bindingKeyName = name;
        }

        public static void HandleBinding()
        {
            KeyCode key = GetPressedKey();

            if (key != KeyCode.None)
            {
                EditBinding(bindingKeyName, key);
                binding = false;
            }
        }

        public static void EditBinding(string name, KeyCode key)
        {
            bindings[name] = key;
            OnKeyRebound?.Invoke();
        }

        public static void SaveBindings()
        {
            Serializer.SaveGlobal(bindings, "ControlBindings");
        }

        public static void LoadBindings()
        {
            if (Serializer.LoadGlobal(out bindings, "ControlBindings"))
            {
                OnKeyRebound?.Invoke();
            }
        }
    }

    public static class Regexer
    {
        public static RegexObject Begin()
        {
            return new RegexObject();
        }
        public static string Finish(this RegexObject regex)
        {
            return regex.sb.ToString();
        }

        public class RegexObject
        {
            public StringBuilder sb;

            public RegexObject()
            {
                sb = new StringBuilder();
            }

            public RegexObject Any()
            {
                sb.Append(".");
                return AnyTimes();
            }
            public RegexObject Any(int count)
            {
                sb.Append(".");
                return Times(count);
            }
            public RegexObject Any(int min, int max)
            {
                sb.Append($".");
                return MinMaxTimes(min, max);
            }

            public RegexObject Exact(string text)
            {
                text = text.AddInFrontOfMatches("\\", @"\", @"^", @"$", @".", @"|", @"?", @"*", @"+", @"(", @")", @"[", @"]", @"{", @"}");
                sb.Append($"{text}");
                return this;
            }

            public RegexObject UppercaseCharacter()
            {
                sb.Append("[A-Z]");
                return AnyTimes();
            }
            public RegexObject UppercaseCharacter(int count)
            {
                sb.Append("[A-Z]");
                return Times(count);
            }
            public RegexObject UppercaseCharacter(int min, int max)
            {
                sb.Append("[A-Z]");
                return MinMaxTimes(min, max);
            }

            public RegexObject LowercaseCharacter()
            {
                sb.Append("[a-z]");
                return AnyTimes();
            }
            public RegexObject LowercaseCharacter(int count)
            {
                sb.Append("[a-z]");
                return Times(count);
            }
            public RegexObject LowercaseCharacter(int min, int max)
            {
                sb.Append("[a-z]");
                return MinMaxTimes(min, max);
            }

            public RegexObject UppercaseOrLowercaseCharacter()
            {
                sb.Append("[a-zA-Z]");
                return AnyTimes();
            }
            public RegexObject UppercaseOrLowercaseCharacter(int count)
            {
                sb.Append("[a-zA-Z]");
                return Times(count);
            }
            public RegexObject UppercaseOrLowercaseCharacter(int min, int max)
            {
                sb.Append("[a-zA-Z]");
                return MinMaxTimes(min, max);
            }

            public RegexObject Number()
            {
                sb.Append("[0-9]");
                return AnyTimes();
            }
            public RegexObject Number(int count)
            {
                sb.Append("[0-9]");
                return Times(count);
            }
            public RegexObject Number(int min, int max)
            {
                sb.Append("[0-9]");
                return MinMaxTimes(min, max);
            }

            public RegexObject NonSymbolCharacter()
            {
                sb.Append("[a-zA-Z0-9]");
                return AnyTimes();
            }
            public RegexObject NonSymbolCharacter(int count)
            {
                sb.Append("[a-zA-Z0-9]");
                return Times(count);
            }
            public RegexObject NonSymbolCharacter(int min, int max)
            {
                sb.Append("[a-zA-Z0-9]");
                return MinMaxTimes(min, max);
            }

            public RegexObject Custom(string custom)
            {
                sb.Append(custom);
                return this;
            }

            public RegexObject AnyTimes()
            {
                sb.Append("*");
                return this;
            }
            public RegexObject Times(int count)
            {
                sb.Append($@"{{{count}}}");
                return this;
            }
            public RegexObject MinMaxTimes(int min, int max)
            {
                sb.Append($@"{{{min},{max}}}");
                return this;
            }
            public RegexObject MinTimes(int count)
            {
                sb.Append($@"{{{count},}}");
                return this;
            }

            public RegexObject Start()
            {
                sb.Append("^");
                return this;
            }
            public RegexObject End()
            {
                sb.Append("$");
                return this;
            }


            public override string ToString() => this.Finish();

            public static implicit operator string (RegexObject regex) => regex.Finish();
        }
    }

    public static class SpringUtils
    {
        static DampedSpringMotionParams motionParams = new DampedSpringMotionParams();

        class DampedSpringMotionParams
        {
            public float posPosCoef, posVelCoef;
            public float velPosCoef, velVelCoef;
        };


        public static void CalculateHarmonicSpringMotion(ref float position, ref float velocity, float goalPos, float frequency = 3.5f, float damping = 0.5f, float deltaTime = float.Epsilon)
        {
            if (deltaTime == float.Epsilon) deltaTime = Time.deltaTime;

            CalculateDampedSpringMotionParams(ref motionParams, deltaTime, frequency, damping);
            UpdateDampedSpringMotion(ref position, ref velocity, goalPos, motionParams);
        }

        static void CalculateDampedSpringMotionParams(ref DampedSpringMotionParams springParams, float deltaTime, float angularFrequency, float dampingRatio)
        {
            if (dampingRatio < 0.0f) dampingRatio = 0.0f;
            if (angularFrequency < 0.0f) angularFrequency = 0.0f;

            if (angularFrequency < float.Epsilon)
            {
                springParams.posPosCoef = 1.0f; springParams.posVelCoef = 0.0f;
                springParams.velPosCoef = 0.0f; springParams.velVelCoef = 1.0f;
                return;
            }


            if (dampingRatio > 1.0f + float.Epsilon)
            {
                // over-damped
                float za = -angularFrequency * dampingRatio;
                float zb = angularFrequency * Mathf.Sqrt(dampingRatio * dampingRatio - 1f);
                float z1 = za - zb;
                float z2 = za + zb;

                float e1 = Mathf.Exp(z1 * deltaTime);
                float e2 = Mathf.Exp(z2 * deltaTime);

                float invTwoZb = 1f / (2f * zb);

                float e1_Over_TwoZb = e1 * invTwoZb;
                float e2_Over_TwoZb = e2 * invTwoZb;

                float z1e1_Over_TwoZb = z1 * e1_Over_TwoZb;
                float z2e2_Over_TwoZb = z2 * e2_Over_TwoZb;

                springParams.posPosCoef = e1_Over_TwoZb * z2 - z2e2_Over_TwoZb + e2;
                springParams.posVelCoef = -e1_Over_TwoZb + e2_Over_TwoZb;

                springParams.velPosCoef = (z1e1_Over_TwoZb - z2e2_Over_TwoZb + e2) * z2;
                springParams.velVelCoef = -z1e1_Over_TwoZb + z2e2_Over_TwoZb;
            }
            else if (dampingRatio < 1f - float.Epsilon)
            {
                // under-damped
                float omegaZeta = angularFrequency * dampingRatio;
                float alpha = angularFrequency * Mathf.Sqrt(1f - dampingRatio * dampingRatio);

                float expTerm = Mathf.Exp(-omegaZeta * deltaTime);
                float cosTerm = Mathf.Cos(alpha * deltaTime);
                float sinTerm = Mathf.Sin(alpha * deltaTime);

                float invAlpha = 1f / alpha;

                float expSin = expTerm * sinTerm;
                float expCos = expTerm * cosTerm;
                float expOmegaZetaSin_Over_Alpha = expTerm * omegaZeta * sinTerm * invAlpha;

                springParams.posPosCoef = expCos + expOmegaZetaSin_Over_Alpha;
                springParams.posVelCoef = expSin * invAlpha;

                springParams.velPosCoef = -expSin * alpha - omegaZeta * expOmegaZetaSin_Over_Alpha;
                springParams.velVelCoef = expCos - expOmegaZetaSin_Over_Alpha;
            }
            else
            {
                // critically damped
                float expTerm = Mathf.Exp(-angularFrequency * deltaTime);
                float timeExp = deltaTime * expTerm;
                float timeExpFreq = timeExp * angularFrequency;

                springParams.posPosCoef = timeExpFreq + expTerm;
                springParams.posVelCoef = timeExp;

                springParams.velPosCoef = -angularFrequency * timeExpFreq;
                springParams.velVelCoef = -timeExpFreq + expTerm;
            }
        }
        static void UpdateDampedSpringMotion(ref float pPos, ref float pVel, float equilibriumPos, in DampedSpringMotionParams springParams)
        {
            float oldPos = pPos - equilibriumPos;
            float oldVel = pVel;

            pPos = oldPos * springParams.posPosCoef + oldVel * springParams.posVelCoef + equilibriumPos;
            pVel = oldPos * springParams.velPosCoef + oldVel * springParams.velVelCoef;
        }
    }

    [Serializable]
    public class Spring
    {
        [SerializeField] float frequency = 3f;
        [SerializeField][Range(0f, 1f)] float damping = 0.5f;
        [SerializeField] float goal = 1f;
        [field: SerializeField] public float Min { get; private set; } = -Mathf.Infinity;
        [field: SerializeField] public float Max { get; private set; } = Mathf.Infinity;

        public float Velocity => velocity;
        float velocity;
        float pos;


        Spring() { } // Stupid workaround so Unity properly serializes the class with the correct default values

        public Spring(float frequency, float damping, float start = 0f)
        {
            this.frequency = frequency;
            this.damping = damping;

            pos = start;
            goal = start;
        }


        public void Update(float deltaTime = float.Epsilon)
        {
            SpringUtils.CalculateHarmonicSpringMotion(ref pos, ref velocity, goal, frequency, damping, deltaTime);
            pos = Mathf.Clamp(pos, Min, Max);
        }

        public float GetValue()
        {
            Update();
            return pos;
        }
        public float GetValueWithoutUpdate() => pos;

        public void SetGoal(float goal) => this.goal = Mathf.Clamp(goal, Min, Max);

        public void AddForce(float force) => velocity += force;
        public void SetVelocity(float velocity) => this.velocity = velocity;
        public void SetValue(float pos) => this.pos = pos;
        public void SetMinMax(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    public class Pool
    {
        Queue<GameObject> poolQueue;

        GameObject poolObject;
        Transform parent;

        public int GrowSize
        {
            get => GrowSize;
            set
            {
                GrowSize = Mathf.Clamp(value, 1, 1000);
            }
        }

        public int maxSize
        {
            get => maxSize;
            set
            {
                maxSize = Mathf.Clamp(value, 1, 1000);
            }
        }
        public int currentSize;
        public int objectsAvailable;

        public Pool(GameObject poolObject, Transform parent, int growSize = 10, int maxSize = 1000)
        {
            poolQueue = new Queue<GameObject>();

            this.poolObject = poolObject;
            this.parent = parent;
            this.GrowSize = growSize;
            this.maxSize = maxSize;

            currentSize = 0;
            objectsAvailable = 0;

            GrowPool();
        }


        public GameObject Get()
        {
            if (objectsAvailable == 0) GrowPool();
            if (objectsAvailable == 0) return null;

            GameObject getObject = poolQueue.Dequeue();
            objectsAvailable--;

            getObject.SetActive(true);
            return getObject;
        }
        public GameObject Instantiate(Vector3 position, Quaternion rotation)
        {
            GameObject getObject = Get();

            if (getObject == null) return null;

            getObject.transform.position = position;
            getObject.transform.rotation = rotation;

            return getObject;
        }

        public void ReturnObject(GameObject returnObject)
        {
            returnObject.SetActive(false);
            if (!poolQueue.Contains(returnObject)) poolQueue.Enqueue(returnObject);

            objectsAvailable++;
        }

        public bool GrowPool()
        {
            if (currentSize >= maxSize) return false;

            for (int i = 0; i < GrowSize && currentSize < maxSize; i++)
            {
                GameObject newObject = MonoBehaviour.Instantiate(poolObject, parent.position, Quaternion.identity);
                newObject.transform.parent = parent;

                newObject.SetActive(false);
                poolQueue.Enqueue(newObject);

                currentSize++;
                objectsAvailable++;
            }

            return true;
        }

        public void EmptyPool()
        {
            currentSize = 0;
            objectsAvailable = 0;

            poolQueue.Clear();
        }
    }

    [Serializable]
    public class LootTable<TTableItem>
    {
        [SerializeField] List<TableItem> bagItems = new List<TableItem>();

        public int TotalWeight { get; private set; } = 0;


        public void AddItem(TTableItem item, int weight)
        {
            bagItems.Add(new TableItem(item, weight));
            CalculateTotalWeight();
        }

        public TTableItem GetItem()
        {
            if (TotalWeight == 0) CalculateTotalWeight();

            int randomWeight = Random.Range(1, TotalWeight + 1);

            foreach (TableItem bagItem in bagItems)
            {
                randomWeight -= bagItem.weight;

                if (randomWeight <= 0) return bagItem.item;
            }

            return default(TTableItem);
        }

        public void CalculateTotalWeight()
        {
            TotalWeight = 0;

            foreach (TableItem item in bagItems) TotalWeight += item.weight;
        }

        public void ClearAllItems()
        {
            bagItems.Clear();
            TotalWeight = 0;
        }

        [Serializable]
        public class TableItem
        {
            public TTableItem item;
            public int weight;

            public TableItem(TTableItem item, int weight)
            {
                this.item = item;
                this.weight = weight;
            }
        }
    }

    namespace Serialization
    {
        public static class Serializer
        {
            public static string SAVEDATA_FOLDER = Path.Combine(Application.persistentDataPath, "SaveData");
            const string SAVE_FOLDER_TEMPLATE = "Save_";
            const string FILE_EXTENSION = ".SAVE";

            const string EMPTY_STRING = "";

            public static SerializationType SerializationType = SerializationType.JSONFormatted;
            public static int CurrentSaveID = 1;


            #region Saving
            public static void SaveGlobal<TSaveType>(TSaveType saveData, string saveKey, string subFolder = EMPTY_STRING)
            {
                switch (SerializationType)
                {
                    case SerializationType.Binary:
                        SaveToDisk(SerializeToBytes(saveData), saveKey, GetGlobalSaveDirectory(subFolder));
                        break;

                    case SerializationType.JSON:
                        SaveJSONToDisk(SerializeObject(saveData, Formatting.None), saveKey, GetGlobalSaveDirectory(subFolder));
                        break;

                    case SerializationType.JSONFormatted:
                        SaveJSONToDisk(SerializeObject(saveData, Formatting.Indented), saveKey, GetGlobalSaveDirectory(subFolder));
                        break;
                }
            }
            public static void SaveGlobal(byte[] saveData, string saveKey, string subFolder = EMPTY_STRING) => SaveToDisk(saveData, saveKey, GetGlobalSaveDirectory(subFolder));

            public static void Save<TSaveType>(TSaveType saveData, string saveKey, string subFolder = EMPTY_STRING)
            {
                switch (SerializationType)
                {
                    case SerializationType.Binary:
                        SaveToDisk(SerializeToBytes(saveData), saveKey, GetSaveDirectory(subFolder));
                        break;

                    case SerializationType.JSON:
                        SaveJSONToDisk(SerializeObject(saveData, Formatting.None), saveKey, GetSaveDirectory(subFolder));
                        break;

                    case SerializationType.JSONFormatted:
                        SaveJSONToDisk(SerializeObject(saveData, Formatting.Indented), saveKey, GetSaveDirectory(subFolder));
                        break;
                }
            }
            public static void Save(byte[] saveData, string saveKey, string subFolder = EMPTY_STRING) => SaveToDisk(saveData, saveKey, GetSaveDirectory(subFolder));

            static void SaveToDisk(byte[] saveData, string saveKey, string path)
            {
                CreateDirectoryIfNeeded(path);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                    {
                        gzipStream.Write(saveData, 0, saveData.Length);
                    }

                    File.WriteAllBytes(GetTotalPath(saveKey, path), memoryStream.ToArray());
                }
            }
            static void SaveJSONToDisk(string JSON, string saveKey, string path)
            {
                CreateDirectoryIfNeeded(path);
                File.WriteAllText(GetTotalPath(saveKey, path), JSON);
            }

            static byte[] SerializeToBytes<TType>(TType data) => System.Text.Encoding.UTF8.GetBytes(SerializeObject(data, Formatting.None));
            static string SerializeObject<TType>(TType data, Formatting formatting) => JsonConvert.SerializeObject(data, formatting);

            static void CreateDirectoryIfNeeded(string path)
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
            #endregion
            #region Loading
            public static bool LoadGlobal<TLoadType>(out TLoadType loadData, string loadKey, string subFolder = EMPTY_STRING)
            {
                return LoadFromDisk(out loadData, loadKey, GetGlobalSaveDirectory(subFolder));
            }
            public static bool LoadGlobal(out byte[] loadData, string loadKey, string subFolder = EMPTY_STRING)
            {
                return LoadFromDisk(out loadData, loadKey, GetGlobalSaveDirectory(subFolder));
            }

            public static bool Load<TLoadType>(out TLoadType loadData, string loadKey, string subFolder = EMPTY_STRING)
            {
                return LoadFromDisk(out loadData, loadKey, GetSaveDirectory(subFolder));
            }
            public static bool Load(out byte[] loadData, string loadKey, string subFolder = EMPTY_STRING)
            {
                return LoadFromDisk(out loadData, loadKey, GetSaveDirectory(subFolder));
            }

            static bool LoadFromDisk<TLoadType>(out TLoadType loadData, string loadKey, string path)
            {
                switch (SerializationType)
                {
                    default:
                        loadData = default;
                        return false;

                    case SerializationType.Binary:

                        if (LoadFromDisk(out byte[] compressedData, loadKey, path))
                        {
                            using (MemoryStream memoryStream = new MemoryStream(compressedData))
                            {
                                using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                                {
                                    using (StreamReader reader = new StreamReader(gzipStream))
                                    {
                                        loadData = JsonConvert.DeserializeObject<TLoadType>(reader.ReadToEnd());
                                        return true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            loadData = default;
                            return false;
                        }

                    case SerializationType.JSON:

                        if (File.Exists(GetTotalPath(loadKey, path)))
                        {
                            string JSON = File.ReadAllText(GetTotalPath(loadKey, path));
                            loadData = JsonConvert.DeserializeObject<TLoadType>(JSON);
                            return true;
                        }
                        else
                        {
                            loadData = default;
                            return false;
                        }

                    case SerializationType.JSONFormatted:

                        if (File.Exists(GetTotalPath(loadKey, path)))
                        {
                            string JSON = File.ReadAllText(GetTotalPath(loadKey, path));
                            loadData = JsonConvert.DeserializeObject<TLoadType>(JSON);
                            return true;
                        }
                        else
                        {
                            loadData = default;
                            return false;
                        }
                }
            }
            static bool LoadFromDisk(out byte[] loadData, string loadKey, string path)
            {
                string totalPath = GetTotalPath(loadKey, path);
                if (File.Exists(totalPath))
                {
                    loadData = File.ReadAllBytes(totalPath);
                    return true;
                }
                else
                {
                    loadData = new byte[0];
                    return false;
                }
            }
            #endregion
            #region Utilities
            static string GetSaveDirectory(string subFolder)
            {
                subFolder = subFolder.Replace('/', Path.DirectorySeparatorChar);
                return Path.Combine(SAVEDATA_FOLDER, $"{SAVE_FOLDER_TEMPLATE}{CurrentSaveID}", subFolder);
            }
            static string GetGlobalSaveDirectory(string subFolder)
            {
                subFolder = subFolder.Replace('/', Path.DirectorySeparatorChar);
                return Path.Combine(SAVEDATA_FOLDER, "Global", subFolder);
            }
            static string GetFileName(string saveKey) => $"{saveKey}{FILE_EXTENSION}";
            static string GetTotalPath(string key, string path) => Path.Combine(path, GetFileName(key));

            static int GetSaveIDFromDirectory(string directory) => int.Parse(directory.Remove(Path.Combine(SAVEDATA_FOLDER, SAVE_FOLDER_TEMPLATE)));
            static string GetDirectoryFromSaveID(int saveID) => $"{Path.Combine(SAVEDATA_FOLDER, SAVE_FOLDER_TEMPLATE)}{saveID}";
            #endregion
            #region Helper Methods
            public static int[] GetSaveIDs()
            {
                string[] saveDirectories = Directory.GetDirectories(SAVEDATA_FOLDER);
                int saveDirectoryCount = saveDirectories.Length;

                int[] saveIDs = new int[saveDirectoryCount];

                for (int i = 0; i < saveDirectoryCount; i++)
                {
                    saveIDs[i] = GetSaveIDFromDirectory(saveDirectories[i]);
                }
                return saveIDs;
            }
            public static int GetHighestAvailableSaveID()
            {
                int saveID = 1;

                while (Directory.Exists(GetDirectoryFromSaveID(saveID))) saveID++;
                return saveID;
            }

            public static void ClearSave(int saveID = -1)
            {
                if (saveID == -1) saveID = CurrentSaveID;

                string directory = GetDirectoryFromSaveID(saveID);
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }

            public static void ClearGlobalSaveData(string saveKey, string subFolder)
            {
                string filePath = GetTotalPath(saveKey, GetGlobalSaveDirectory(subFolder));
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            public static void ClearSaveData(string saveKey, string subFolder)
            {
                string filePath = GetTotalPath(saveKey, GetSaveDirectory(subFolder));
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            #endregion
        }

        public enum SerializationType
        {
            Binary,
            JSON,
            JSONFormatted
        }
    }

    namespace Commands
    {
        public static class CommandSystem
        {
            public static List<DebugCommandBase> commandList = new List<DebugCommandBase>();

            public static void AddCommand(DebugCommandBase command)
            {
                if (command.commandName.Contains(" ")) command.commandName.Replace(" ", "_");
                commandList.Add(command);
            }
        }

        public abstract class DebugCommandBase : IComparable<DebugCommandBase>
        {
            public string commandName;

            public Type[] types;

            public bool hidden;

            public DebugCommandBase(string commandName, bool hidden = false)
            {
                this.commandName = commandName;
                this.hidden = hidden;
            }

            public abstract bool TryExecute(int parameterCount, string[] properties);

            public int CompareTo(DebugCommandBase other)
            {
                return string.Compare(commandName, other.commandName);
            }
        }

        public class DebugCommand : DebugCommandBase
        {
            Action command;

            public DebugCommand(string commandName, Action command, bool hidden = false) : base(commandName, hidden)
            {
                this.command = command;
                types = new Type[0];
            }

            public void Execute()
            {
                command.Invoke();
            }

            public override bool TryExecute(int parametercount, string[] properties)
            {
                command.Invoke();
                return true;
            }
        }

        public class DebugCommand<T1> : DebugCommandBase
        {
            Action<T1> command;

            public DebugCommand(string commandName, Action<T1> command, bool hidden = false) : base(commandName, hidden)
            {
                this.command = command;
                types = new Type[] { typeof(T1) };
            }

            public void Execute(T1 value)
            {
                command.Invoke(value);
            }

            public override bool TryExecute(int parameterCount, string[] properties)
            {
                if (parameterCount != types.Length) return false;

                Execute((T1)Convert.ChangeType(properties[1], typeof(T1)));
                return true;
            }
        }

        public class DebugCommand<T1, T2> : DebugCommandBase
        {
            Action<T1, T2> command;

            public DebugCommand(string commandName, Action<T1, T2> command, bool hidden = false) : base(commandName, hidden)
            {
                this.command = command;
                types = new Type[] { typeof(T1), typeof(T2) };
            }

            public void Execute(T1 value, T2 value2)
            {
                command.Invoke(value, value2);
            }

            public override bool TryExecute(int parameterCount, string[] properties)
            {
                if (parameterCount != types.Length) return false;

                Execute((T1)Convert.ChangeType(properties[1], typeof(T1)), (T2)Convert.ChangeType(properties[2], typeof(T2)));
                return true;
            }
        }
    }

    namespace QOL
    {
        public static class QOL
        {
            public const float Tau = Mathf.PI * 2;

            public static bool Raycast2D(Vector2 origin, Vector2 direction, out RaycastHit2D hit, float distance)
            {
                return hit = Physics2D.Raycast(origin, direction, distance);
            }
            public static bool Raycast2D(Vector2 origin, Vector2 direction, out RaycastHit2D hit, float distance, int layerMask)
            {
                return hit = Physics2D.Raycast(origin, direction, distance, layerMask);
            }

            public static void CopyToClipboard(string text)
            {
                GUIUtility.systemCopyBuffer = text;
            }


            static Dictionary<float, WaitForSeconds> waitDictionary = new Dictionary<float, WaitForSeconds>();
            static WaitForSeconds waitCache;
            public static WaitForSeconds GetWaitForSeconds(float time)
            {
                if (waitDictionary.TryGetValue(time, out waitCache)) return waitCache;
                return waitDictionary[time] = new WaitForSeconds(time);
            }
            public static void PurgeWaitCache() => waitDictionary.Clear();

            static Dictionary<float, WaitForSecondsRealtime> unscaledWaitDictionary = new Dictionary<float, WaitForSecondsRealtime>();
            static WaitForSecondsRealtime unscaledWaitCache;
            public static WaitForSecondsRealtime GetUnscaledWaitForSeconds(float time)
            {
                if (unscaledWaitDictionary.TryGetValue(time, out unscaledWaitCache)) return unscaledWaitCache;
                return unscaledWaitDictionary[time] = new WaitForSecondsRealtime(time);
            }
            public static void PurgeUnscaledWaitCache() => unscaledWaitDictionary.Clear();
        }

        [Flags]
        public enum Direction
        {
            None = 0,
            Up = 1,
            Down = 2,
            Left = 4,
            Right = 8,
            Forward = 16,
            Back = 32
        }
    }
}