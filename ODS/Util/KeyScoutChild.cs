using System;
using System.Collections.Generic;
using System.Text;

namespace ODS.Util
{
    class KeyScoutChild
    {
        private string name;
        private int size;
        private int startingIndex;

        public KeyScoutChild()
        {
            size = 0;
            startingIndex = -1;
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public string GetName()
        {
            return name;
        }

        public void SetSize(int size)
        {
            this.size = size;
        }

        public int GetSize()
        {
            return size;
        }

        public void SetStartingIndex(int startingIndex)
        {
            this.startingIndex = startingIndex;
        }

        public int GetStartingIndex()
        {
            return startingIndex;
        }

        internal void RemoveSize(int amount)
        {
            size -= amount;
        }

        internal void AddSize(int amount)
        {
            size += amount;
        }
    }
}
