using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CondorSubmitGUI.Objects.Ortho
{
    class ATBlock
    {
        public List<string> blockPhotos = new List<string>();
        public string blockName;
        public ATBlock(string blockName)
        {
            this.blockName = blockName;
        }
    }
}
