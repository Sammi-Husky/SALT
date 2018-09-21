using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace SALT.Moveset.AnimCMD
{
    public class ACMDDecompiler
    {
        public Dictionary<int, BasicBlock> Blocks = new Dictionary<int, BasicBlock>();
        public bool isBranch(ICommand cmd)
        {
            switch (cmd.Ident)
            {
                case 0x895B9275:
                case 0x870CF021:
                case 0xA5BD4F32:
                case 0x38A3EC78:
                    return true;

                case 0x5766F889: // script_end
                    return true;

                default:
                    return false;
            }
        }
        public void decompile(ACMDScript script)
        {
            for (int i = 0; i < script.Count; i++)
            {
                var blk = MakeBlock(null, script, i);
                Blocks.Add(i, blk);
                i += blk.Commands.Count;
                
            }
        }
        public BasicBlock MakeBlock(BasicBlock parent, ACMDScript script, int startIndex)
        {
            var block = new BasicBlock();
            block.ID = startIndex;

            if (parent != null)
                block.Callers.Add(parent);

            int i = startIndex;
            while (!isBranch(script[i]))
            {
                block.Commands.Add(script[i]);
                i++;
            }
            var cmd = (ACMDCommand)script[i];
            block.Commands.Add(cmd);

            if (script[i].Ident != 0x5766F889)
                block.Callee = MakeBlock(block, script, i + (int)cmd.Parameters[0]);

            return block;
        }
    }
    public class BasicBlock
    {
        public BasicBlock()
        {
            Callers = new List<BasicBlock>();
            Commands = new List<ICommand>();
        }
        public int ID { get; set; }
        public List<BasicBlock> Callers { get; set; }
        public BasicBlock Callee { get; set; }

        public List<ICommand> Commands { get; set; }
    }
}
