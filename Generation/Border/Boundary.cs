
    using System;
    using System.Collections.Generic;

    public class Boundary<TPrim> : Chain<Segment<TPrim>, TPrim>, IBoundary<TPrim>
    {
        public Action<TPrim> CrossedSelf { get; set; }

        protected Boundary(List<Segment<TPrim>> elements) : base(elements)
        {
        }

    }
