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
        public SortedList<int, BasicBlock> BlockList = new SortedList<int, BasicBlock>();
        public List<int> JumpLocations = new List<int>();
        public ACMDScript TargetScript { get; set; }

        public void Decompile(ACMDScript script)
        {
            TargetScript = script;
            BlockList.Clear();
            FindJumpLocations();
            MakeBlocks();
            StructureConditionals();
        }
        public void MakeBlocks()
        {
            var blk = GetBlockAt(null, 0);
            int current_address = 0;
            while (current_address != TargetScript.Count)
            {
                int next_branch = FindNextBranch(current_address);
                if (next_branch == current_address)
                    break;

                BasicBlock next_block = GetBlockAt(blk, GetJumpTarget(next_branch));
                blk = next_block;
                current_address = next_branch + 1;
            }
        }
        public BasicBlock GetBlockAt(BasicBlock parent, int startIndex)
        {
            if (BlockList.ContainsKey(startIndex))
                return BlockList[startIndex];

            var block = new BasicBlock();
            block.ID = startIndex;
            block.Predecessors.Add(parent);


            int label = JumpLocations.FirstOrDefault(x => x > startIndex);
            if (label == 0)
                label = JumpLocations.Last();

            int branch = FindNextBranch(startIndex);
            int blockAfterBranch = branch + 1;

            if (branch == startIndex)
            {
                // if no branches in or after this block
                block.Length = TargetScript.Count - startIndex;
            }
            else if (label < blockAfterBranch)
            {
                block.Length = label - startIndex;
            }
            else
            {
                block.Length = blockAfterBranch - startIndex;
            }

            for (int i = 0; i < block.Length; i++)
                block.Commands.Add(TargetScript[startIndex + i]);

            int nextBlockIndex = startIndex + block.Length;
            int endIndex = nextBlockIndex - 1;

            // the block if branch is taken
            if (IsBranch(block.Commands.Last()))
                block.Successors.Add(GetBlockAt(block, GetJumpTarget(endIndex)));


            // the block if branch is not taken
            if (nextBlockIndex < TargetScript.Count &&
                block.Commands.Last().Ident != 0x895B9275)
            {
                block.Successors.Add(GetBlockAt(block, nextBlockIndex));
            }
            BlockList.Add(startIndex, block);

            return block;
        }

        public bool IsBranch(ICommand cmd)
        {
            switch (cmd.Ident)
            {
                case 0x895B9275: // Else
                case 0x870CF021: // False
                case 0xA5BD4F32: // True
                case 0x47810508: // unk_47810508
                case 0xC31DF569: // unk_C31DF569
                case 0x38A3EC78: // Goto
                    return true;

                default:
                    return false;
            }
        }
        public int FindNextBranch(int startIndex)
        {
            for (int i = startIndex; i < TargetScript.Count; i++)
            {
                if (IsBranch(TargetScript[i]))
                    return i;
            }
            return startIndex;
        }

        public void FindJumpLocations()
        {
            JumpLocations.Clear();
            for (int i = 0; i < TargetScript.Count; i++)
            {
                var cmd = TargetScript[i];
                if (IsBranch(cmd))
                {
                    int loc = GetJumpTarget(i);
                    if (!JumpLocations.Contains(loc))
                        JumpLocations.Add(loc);
                }
            }
            JumpLocations.Sort();
        }
        public int GetJumpTarget(int startIndex)
        {
            int i = startIndex;

            int len = (int)TargetScript[startIndex].Parameters[0];
            len -= TargetScript[i].Size / 4;
            i++;

            while (len > 0)
            {
                len -= TargetScript[i].Size / 4;
                if (IsBranch(TargetScript[i]))
                {
                    int loc = GetJumpTarget(i);
                    if (!JumpLocations.Contains(loc))
                        JumpLocations.Add(loc);
                }

                i++;
            }
            return i;
        }

        public void StructureConditionals()
        {
            foreach (var block in BlockList.Values)
            {
                if (StructureIfElse(block))
                    continue;
                if (StructureIfThen(block))
                    continue;
            }
        }
        public bool StructureIfElse(BasicBlock block)
        {
            if (block.Successors.Count != 2)
                return false;

            BasicBlock trueBlock = block.Successors[0];
            BasicBlock falseBlock = block.Successors[1];

            if (trueBlock.Successors.Count != 1 ||
                falseBlock.Successors.Count != 1)
                return false;

            if (falseBlock.Successors[0] != trueBlock.Successors[0])
                return false;

            var statement = new IfElseStatement();
            statement.TrueBlock = block.Successors[0];
            statement.FalseBlock = block.Successors[1];

            return true;
        }
        public bool StructureIfThen(BasicBlock block)
        {
            if (block.Successors.Count != 2)
                return false;

            BasicBlock trueBlock = block.Successors[0];
            BasicBlock falseBlock = block.Successors[1];

            var statement = new IfElseStatement();
            statement.TrueBlock = block.Successors[0];
            statement.FalseBlock = null;

            return true;
        }
    }
    public class BasicBlock
    {
        public BasicBlock()
        {
            Predecessors = new List<BasicBlock>();
            Successors = new List<BasicBlock>();
            Commands = new List<ICommand>();
        }

        public int ID { get; set; }
        public int Length { get; set; }
        public List<BasicBlock> Predecessors { get; set; }
        public List<BasicBlock> Successors { get; set; }

        public List<ICommand> Commands { get; set; }

        public override string ToString()
        {
            return $"{ID} {Predecessors.Count} {Successors.Count}";
        }
    }
    public class IfElseStatement
    {
        public BasicBlock TrueBlock { get; set; }
        public BasicBlock FalseBlock { get; set; }
    }
}
