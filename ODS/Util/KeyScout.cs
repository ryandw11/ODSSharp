using System;
using System.Collections.Generic;
using System.Text;

namespace ODS.Util
{
    class KeyScout
    {
        private List<KeyScoutChild> children;
        private KeyScoutChild end;

        public KeyScout()
        {
            children = new List<KeyScoutChild>();
        }

        public void AddChild(KeyScoutChild child)
        {
            children.Add(child);
        }

        public List<KeyScoutChild> GetChildren()
        {
            return children;
        }

        public KeyScoutChild GetChildByName(string name)
        {
            foreach (KeyScoutChild child in children)
            {
                if (child.GetName() == name)
                    return child;
            }
            return null;
        }

        public KeyScoutChild GetEnd()
        {
            return end;
        }

        public void SetEnd(KeyScoutChild end)
        {
            this.end = end;
        }

        public void RemoveAmount(int size)
        {
            foreach (KeyScoutChild child in children)
            {
                child.RemoveSize(size);
            }
        }

        public void AddAmount(int size)
        {
            foreach (KeyScoutChild child in children)
            {
                child.AddSize(size);
            }
        }


    }
}
