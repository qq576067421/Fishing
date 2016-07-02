using System;
using ProtoBuf;

namespace GF.Common
{
    [Serializable]
    [ProtoContract]
    public struct EbVector3 : IEquatable<EbVector3>
    {
        //---------------------------------------------------------------------
        [ProtoMember(1)]
        public float x { get; set; }// Gets or sets the X value.
        [ProtoMember(2)]
        public float y { get; set; }// Gets or sets Y value.
        [ProtoMember(3)]
        public float z { get; set; }// Gets or sets Z value.
        public float Length { get { return (float)Math.Sqrt((x * x) + (y * y) + (z * z)); } }
        public float Length2 { get { return ((x * x) + (y * y) + (z * z)); } }

        //---------------------------------------------------------------------
        public EbVector3(float x, float y, float z)
            : this()
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        //---------------------------------------------------------------------
        public static EbVector3 Zero
        {
            get
            {
                EbVector3 v3 = new EbVector3();
                v3.x = 0.0f;
                v3.y = 0.0f;
                v3.z = 0.0f;
                return v3;
            }
        }

        //---------------------------------------------------------------------
        public static EbVector3 UnitX
        {
            get
            {
                EbVector3 v3 = new EbVector3();
                v3.x = 1.0f;
                v3.y = 0.0f;
                v3.z = 0.0f;
                return v3;
            }
        }

        //---------------------------------------------------------------------
        public static EbVector3 UnitY
        {
            get
            {
                EbVector3 v3 = new EbVector3();
                v3.x = 0.0f;
                v3.y = 1.0f;
                v3.z = 0.0f;
                return v3;
            }
        }

        //---------------------------------------------------------------------
        public static EbVector3 UnitZ
        {
            get
            {
                EbVector3 v3 = new EbVector3();
                v3.x = 0.0f;
                v3.y = 0.0f;
                v3.z = 1.0f;
                return v3;
            }
        }

        //---------------------------------------------------------------------
        public static EbVector3 Unit
        {
            get
            {
                EbVector3 v3 = new EbVector3();
                v3.x = 1.0f;
                v3.y = 1.0f;
                v3.z = 1.0f;
                return v3;
            }
        }

        //---------------------------------------------------------------------
        public bool Equals(EbVector3 other)
        {
            return this.x.Equals(other.x) && this.y.Equals(other.y) && this.z.Equals(other.z);
        }

        //---------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            if (obj is EbVector3)
            {
                var other = (EbVector3)obj;
                return this.Equals(other);
            }

            return false;
        }

        //---------------------------------------------------------------------
        public override int GetHashCode()
        {
            int result = this.x.GetHashCode();
            result ^= this.y.GetHashCode();
            result ^= this.z.GetHashCode();
            return result;
        }

        //---------------------------------------------------------------------
        public override string ToString()
        {
            return string.Format("{0}({1},{2},{3})", base.ToString(), this.x, this.y, this.z);
        }

        //---------------------------------------------------------------------
        public static bool operator ==(EbVector3 coordinate1, EbVector3 coordinate2)
        {
            return coordinate1.Equals(coordinate2);
        }

        //---------------------------------------------------------------------
        public static bool operator !=(EbVector3 coordinate1, EbVector3 coordinate2)
        {
            return coordinate1.Equals(coordinate2) == false;
        }

        //---------------------------------------------------------------------
        public static EbVector3 operator +(EbVector3 a, EbVector3 b)
        {
            return new EbVector3 { x = a.x + b.x, y = a.y + b.y, z = a.z + b.z };
        }

        //---------------------------------------------------------------------
        public static EbVector3 operator -(EbVector3 a, EbVector3 b)
        {
            return new EbVector3 { x = a.x - b.x, y = a.y - b.y, z = a.z - b.z };
        }

        //---------------------------------------------------------------------
        public static EbVector3 operator -(EbVector3 a)
        {
            return new EbVector3 { x = -a.x, y = -a.y, z = -a.z };
        }

        //---------------------------------------------------------------------
        public static EbVector3 operator *(EbVector3 a, int b)
        {
            return new EbVector3 { x = a.x * b, y = a.y * b, z = a.z * b };
        }

        //---------------------------------------------------------------------
        public static EbVector3 operator *(EbVector3 a, float b)
        {
            return new EbVector3 { x = a.x * b, y = a.y * b, z = a.z * b };
        }

        //---------------------------------------------------------------------
        public static EbVector3 operator /(EbVector3 a, int b)
        {
            return new EbVector3 { x = a.x / b, y = a.y / b, z = a.z / b };
        }

        //---------------------------------------------------------------------
        public static EbVector3 operator /(EbVector3 a, float b)
        {
            return new EbVector3 { x = a.x / b, y = a.y / b, z = a.z / b };
        }

        //---------------------------------------------------------------------
        public static EbVector3 max(EbVector3 value1, EbVector3 value2)
        {
            return new EbVector3 { x = Math.Max(value1.x, value2.x), y = Math.Max(value1.y, value2.y), z = Math.Max(value1.z, value2.z) };
        }

        //---------------------------------------------------------------------
        public static EbVector3 min(EbVector3 value1, EbVector3 value2)
        {
            return new EbVector3 { x = Math.Min(value1.x, value2.x), y = Math.Min(value1.y, value2.y), z = Math.Min(value1.z, value2.z) };
        }

        //---------------------------------------------------------------------
        public static EbVector3 lerp(EbVector3 from, EbVector3 to, float t)
        {
            EbVector3 v = from;
            v += (to - from) * t;
            return v;
        }

        //---------------------------------------------------------------------
        public static float dot(EbVector3 lhs, EbVector3 rhs)
        {
            return (((lhs.x * rhs.x) + (lhs.y * rhs.y)) + (lhs.z * rhs.z));
        }

        //---------------------------------------------------------------------
        public static EbVector3 cross(EbVector3 v1, EbVector3 v2)
        {
            EbVector3 result = new EbVector3();
            result.x = (v1.y * v2.z) - (v1.z * v2.y);
            result.y = (v1.z * v2.x) - (v1.x * v2.z);
            result.z = (v1.x * v2.y) - (v1.y * v2.x);
            return result;
        }

        //---------------------------------------------------------------------
        public static EbVector3 project(EbVector3 vector, EbVector3 on_normal)
        {
            float num = dot(on_normal, on_normal);
            if (num < float.Epsilon)
            {
                return Zero;
            }
            return (EbVector3)((on_normal * dot(vector, on_normal)) / num);
        }

        //---------------------------------------------------------------------
        public float getDistance(EbVector3 vector)
        {
            return (float)Math.Sqrt(Math.Pow(vector.x - this.x, 2) + Math.Pow(vector.y - this.y, 2) + Math.Pow(vector.z - this.z, 2));
        }

        //---------------------------------------------------------------------
        public static float magnitude(EbVector3 a)
        {
            return (float)Math.Sqrt(((a.x * a.x) + (a.y * a.y)) + (a.z * a.z));
        }

        //---------------------------------------------------------------------
        public void normalize()
        {
            float num = magnitude(this);
            if (num > 1E-05f)
            {
                this = (EbVector3)(this / num);
            }
            else
            {
                this = Zero;
            }
        }

        //---------------------------------------------------------------------
        public static EbVector3 normalize(EbVector3 value)
        {
            float num = magnitude(value);
            if (num > 1E-05f)
            {
                return (EbVector3)(value / num);
            }
            return Zero;
        }

        //---------------------------------------------------------------------
        public EbVector3 normalized
        {
            get
            {
                EbVector3 v = new EbVector3(x, y, z);
                float num = magnitude(v);
                if (num > 1E-05f)
                {
                    return (EbVector3)(v / num);
                }
                return Zero;
            }
        }
    }
}