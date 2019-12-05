using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public class IntCodeComputer
    {
        public int[] Memory { get; private set; }

        public int InstructionPointer { get; private set; }
        public int StepCount { get; private set; }

        public List<int> Inputs { get; } = new List<int>();
        public List<int> Outputs { get; } = new List<int>();

        public IntCodeComputer(int[] initialState)
        {
            Memory = Clone(initialState);
        }

        public IntCodeComputer(int[] initialState, IDictionary<int, int> stateOverride) : this(initialState)
        {
            foreach (var pair in stateOverride)
            {
                Memory[pair.Key] = pair.Value;
            }
        }
        static int[] Clone(int[] array)
        {
            var result = new int[array.Length];
            Buffer.BlockCopy(array, 0, result, 0, array.Length * sizeof(int));
            return result;
        }

        public void Run(int maxSteps = int.MaxValue)
        {
            while (maxSteps-- > 0)
            {
                StepCount++;
                var op = GetOpCode();
                switch (op)
                {
                    case OpCode.Add:
                        ExecuteInstruction((a, b) => a + b);
                        break;
                    case OpCode.Mul:
                        ExecuteInstruction((a, b) => a * b);
                        break;
                    case OpCode.Load:
                        break;
                    case OpCode.Store:
                        break;
                    case OpCode.Halt:
                        return;
                    default:
                        throw new InvalidOperationException("Unknown opcode");
                }
            }
        }

        private void ExecuteInstruction(Func<int, int, int> action)
        {
            var arg1 = Memory[Memory[InstructionPointer++]];
            var arg2 = Memory[Memory[InstructionPointer++]];
            var result = action(arg1, arg2);
            Memory[Memory[InstructionPointer++]] = result;
        }

        //private void ExecuteInstruction(Func<int, int> action)
        //{
        //    var arg1 = Memory[Memory[InstructionPointer++]];
        //    var result = action(arg1);
        //    Memory[Memory[InstructionPointer++]] = result;
        //}

        private OpCode GetOpCode() => (OpCode)Memory[InstructionPointer++];
    }

    public enum OpCode
    {
        Add = 1,
        Mul = 2,
        Load = 3,
        Store = 4,
        Halt = 99
    }
}
