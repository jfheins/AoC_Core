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

        private int GetInput() => Inputs[_inputIdx++];

        private void SetOutput(int value) => Outputs.Add(value);

        public void Run(int maxSteps = int.MaxValue)
        {
            while (maxSteps-- > 0)
            {
                StepCount++;
                var instruction = GetInstruction();

                switch (instruction.OpCode)
                {
                    case OpCode.Add:
                        ExecuteInstruction(instruction, (a, b) => a + b);
                        break;
                    case OpCode.Mul:
                        ExecuteInstruction(instruction, (a, b) => a * b);
                        break;
                    case OpCode.Load:
                        ExecuteInstruction(GetInput);
                        break;
                    case OpCode.Store:
                        var arg0 = Memory[InstructionPointer++];
                        SetOutput(Memory[arg0]);
                        break;

                    case OpCode.JmpIfTrue:
                        _ = EvaluateInstruction(instruction, (a, b) => a != 0 ? InstructionPointer = b : 0);
                        break;
                    case OpCode.JmpIfFalse:
                        _ = EvaluateInstruction(instruction, (a, b) => a == 0 ? InstructionPointer = b : 0);
                        break;

                    case OpCode.LessThan:
                        ExecuteInstruction(instruction, (a, b) => a < b ? 1 : 0);
                        break;
                    case OpCode.Equals:
                        ExecuteInstruction(instruction, (a, b) => a == b ? 1 : 0);
                        break;

                    case OpCode.Halt:
                        return;
                    default:
                        throw new InvalidOperationException("Unknown opcode: " + instruction);
                }
            }
        }

        private int GetNextArg(Instruction instruction)
        {
            var paramIndex = InstructionPointer - (instruction.Location + 1);
            var argument = Memory[InstructionPointer++];

            return (instruction.ParameterModes[paramIndex]) switch
            {
                ParameterMode.PositionMode => Memory[argument],
                ParameterMode.ImmediateMode => argument,
                _ => throw new InvalidOperationException("Unknown parameter mode: " + instruction.ParameterModes[paramIndex]),
            };
        }

        private void ExecuteInstruction(Instruction instruction, Func<int, int, int> action)
            => Memory[Memory[InstructionPointer++]] = EvaluateInstruction(instruction, action);

        private int EvaluateInstruction(Instruction instruction, Func<int, int, int> action)
            => action(GetNextArg(instruction), GetNextArg(instruction));

        private void ExecuteInstruction(Func<int> action)
        {
            Memory[Memory[InstructionPointer++]] = action();
        }


        private Instruction GetInstruction() => new Instruction(Memory[InstructionPointer], InstructionPointer++);

        private struct Instruction
        {
            public OpCode OpCode { get; }
            public readonly ParameterMode[] ParameterModes;

            public readonly int Location { get; }

            public Instruction(int instructionCode, int location)
            {
                Location = location;

                OpCode = (OpCode)(instructionCode % 100);
                instructionCode /= 100;
                ParameterModes = new ParameterMode[4];

                for (int i = 0; i < ParameterModes.Length; i++)
                {
                    ParameterModes[i] = (ParameterMode)(instructionCode % 10);
                    instructionCode /= 10;
                }
            }

            public override string ToString() => $"{OpCode} at {Location}";
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
}