using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory
{
    public class AoBInputBytes
    {
        public AoBInputBytes(string AoBName, string[] Sig, byte[] Mask)
        {
            this.AoBName = AoBName;
            this.Sig = Sig;
            this.Mask = Mask;
        }
        public string AoBName { get; private set; }
        public string[] Sig { get; private set; }
        public byte[] Mask { get; private set; }
    }

    public class AoBInput : Dictionary<string, string>
    {
        public new string this[string AoBName]
        {
            get
            {
                return base[AoBName];
            }
        }
        public new void Remove(string AoBName)
        {
            base.Remove(AoBName);
        }
        public new void Add(string AoBName, string AoBSig)
        {
            base.Add(AoBName, AoBSig);
        }
    }

    public class AoBOutput : Dictionary<string, List<long>>
    {
        public new List<long> this[string AoBName]
        {
            get
            {
                return base[AoBName];
            }
        }
        public new void Add(string AoBName, List<long> Result)
        {
            if (base.ContainsKey(AoBName))
                base[AoBName].AddRange(Result);
            else
                base.Add(AoBName, Result);
        }

        public void Add(string AoBName, long[] Result)
        {
            if (base.ContainsKey(AoBName))
                base[AoBName].AddRange(Result);
            else
                base.Add(AoBName, new List<long>(Result));
        }
    }
}
