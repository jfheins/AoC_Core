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

        public void Run(int maxSteps = int.MaxValue)
        {
            while (maxSteps-- > 0)
            {
                StepCount++;
                var op = GetInstruction();
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

        private int GetNextArgument(ParameterMode mode)
        {
            var argument = Memory[InstructionPointer++];

            if (mode == ParameterMode.PositionMode)
                return Memory[argument];

            if (mode == ParameterMode.ImmediateMode)
                return argument;

            throw new InvalidOperationException("Unknown parameter mode: " + mode);
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


        private Instruction GetInstruction()
        {
            Memory.AsMemory(c)
        }


        private OpCode GetOpCode()
        {
            var instruction = Memory[InstructionPointer++];


            return opcode;
        }

        private struct Instruction
        {
            public OpCode OpCode { get; }
            public readonly ParameterMode[] _parameterModes;

            public int Location { get; }

            public Instruction(int location, int instruction)
            {
                Location = location;
                OpCode = (OpCode)(instruction % 100);
                instruction /= 100;
                _parameterModes = new ParameterMode[4];
                for (var i = 0; i < _parameterModes.Length; i++)
                {
                    _parameterModes[i] = (ParameterMode)(instruction % 10);
                    instruction /= 10;
                }
            }

            public override string ToString()
            {
                return $"{OpCode} at {Location}";
            }
        }
    }



    public enum ParameterMode
    {
        PositionMode = 0,
        ImmediateMode = 1
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
