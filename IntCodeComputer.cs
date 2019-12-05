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

        private int _inputIdx = 0;
        private int getInput() => Inputs[_inputIdx++];

        private void setOutput(int value) => Outputs.Add(value);

        private int[] _parameterModes = new int[4];

        public void Run(int maxSteps = int.MaxValue)
        {
            while (maxSteps-- > 0)
            {
                StepCount++;
                var op = GetOpCode();
                int arg0, arg1;
                switch (op)
                {
                    case OpCode.Add:
                        ExecuteInstruction((a, b) => a + b);
                        break;
                    case OpCode.Mul:
                        ExecuteInstruction((a, b) => a * b);
                        break;
                    case OpCode.Load:
                        arg0 = Memory[InstructionPointer++];
                        Memory[arg0] = getInput();
                        break;
                    case OpCode.Store:
                        arg0 = Memory[InstructionPointer++];
                        setOutput(Memory[arg0]);
                        break;

                    case OpCode.JmpIfTrue:
                        arg0 = GetArgument(_parameterModes[0]);
                        arg1 = GetArgument(_parameterModes[1]);
                        if (arg0 != 0)
                            InstructionPointer = arg1;
                        break;

                    case OpCode.JmpIfFalse:
                        arg0 = GetArgument(_parameterModes[0]);
                        arg1 = GetArgument(_parameterModes[1]);
                        if (arg0 == 0)
                            InstructionPointer = arg1;
                        break;

                    case OpCode.LessThan:
                        ExecuteInstruction((a, b) => a < b ? 1 : 0);
                        break;
                    case OpCode.Equals:
                        ExecuteInstruction((a, b) => a == b ? 1 : 0);
                        break;
                    case OpCode.Halt:
                        return;
                    default:
                        throw new InvalidOperationException("Unknown opcode: " + op);
                }
            }
        }

        private int GetArgument(int mode)
        {
            var argument = Memory[InstructionPointer++];
            if (mode == 0)
            {
                return Memory[argument];
            }
            else if (mode == 1)
            {
                return argument;
            }
            return 0;
        }

        private void ExecuteInstruction(Func<int, int, int> action)
        {
            var arg1 = GetArgument(_parameterModes[0]);
            var arg2 = GetArgument(_parameterModes[1]);
            var result = action(arg1, arg2);
            Memory[Memory[InstructionPointer++]] = result;
        }

        //private void ExecuteInstruction(Func<int, int> action)
        //{
        //    var arg1 = Memory[Memory[InstructionPointer++]];
        //    var result = action(arg1);
        //    Memory[Memory[InstructionPointer++]] = result;
        //}


        private OpCode GetOpCode()
        {
            var instruction = Memory[InstructionPointer++];
            var opcode = (OpCode)(instruction % 100);
            instruction /= 100;

            for (var i = 0; i < _parameterModes.Length; i++)
            {
                _parameterModes[i] = instruction % 10;
                instruction /= 10;
            }

            return opcode;
        }
    }

    public enum OpCode
    {
        Add = 1,
        Mul = 2,
        Load = 3,
        Store = 4,
        JmpIfTrue = 5,
        JmpIfFalse = 6,
        LessThan = 7,
        Equals = 8,
        Halt = 99
    }
}
