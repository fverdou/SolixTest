using System;
using System.Collections.Generic;
using System.Text;

namespace SolixTest.Models
{
    public class InspectionPoint
    {
        public InspectionPoint(float x, float y, string code)
        {
            this.x = x;
            this.y = y;
            this.code = code;
        }
        public float x { get; set; }
        public float y { get; set; }
        public String code { get; set; }
    }
}
