using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class RouteObject
    {
        public class Point
        {
            public float x;
            public float y;

            public Point() { }
            public Point(float x, float y) { this.x = x; this.y = y; }
        }

        public class Velocity
        {
            public float speed;
            public float direction;

            public Velocity() { }
            public Velocity(float speed, float direction) { this.speed = speed; this.direction = direction; }

            public override string ToString()
            {
                return "[speed:" + speed + ",direction:" + direction + "]"; ;
            }
        }

        public class AccObject
        {
            public float start_time;//begin from 0
            public Velocity acc;
            public float duration;

            public AccObject() { }
            //public AccObject(float start_time, Velocity acc, float duration)
            //{
            //    this.start_time = start_time;
            //    this.acc = acc;
            //    this.duration = duration;
            //}

            public AccObject(float start_time, float speed, float direction, float duration)
            {
                this.start_time = start_time;
                this.acc = new Velocity(speed, direction);
                this.duration = duration;
            }

            public override string ToString()
            {
                return "[start_time:" + start_time + ",acc:" + acc.ToString() + ",duration:" + duration + "]";
            }
        }
        public Point initPos;
        public Velocity initVelocity;
        public List<AccObject> accs = new List<AccObject>();

        public override string ToString()
        {
            string str = "";
            str += "initPos(" + initPos.x + "," + initPos.y + ")\n";
            str += "initVelocity" + initVelocity.ToString() + "\n";
            foreach (var it in accs)
            {
                str += it.ToString() + "\n";
            }
            return str;
        }
    }
}
