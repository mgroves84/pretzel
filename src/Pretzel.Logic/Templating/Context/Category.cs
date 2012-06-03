using System.Collections.Generic;

namespace Pretzel.Logic.Templating.Context
{
    public class Category 
    {
        public IEnumerable<Page> Posts { get; set; }
        public string Name { get; set; }

        public Category()
        {
            Posts = new List<Page>();
        }
    }
}