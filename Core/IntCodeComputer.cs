using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Core
{
    public class IntCodeComputer
    {
        public int[] Memory => _memory.Values.ToArray();
        public Dictionary<int, int> _memory { get; private set; }

        public int InstructionPointer { get; private set; }
        public int StepCount { get; private set; }

        public Queue<int> Inputs { get; } = new Queue<int>();
        public Queue<int> Outputs { get; } = new Queue<int>();

        private Instruction _currentInstruction;
        private int _relativeBase = 0;

        public OpCode CurrentOpcode => _currentInstruction.OpCode;

        public IntCodeComputer(int[] initialState)
        {
            _memory = initialState.Select((a, i) => (data: a, index: i))
                .ToDictionary(x => x.index, x => x.data);
        }

        public IntCodeComputer(int[] initialState, int firstInput) : this(initialState)
        {
            Inputs.Enqueue(firstInput);
        }

        public IntCodeComputer(int[] initialState, IDictionary<int, int> stateOverride) : this(initialState)
        {
            Contract.Assert(stateOverride != null);
            foreach (var pair in stateOverride)
            {
                _memory[pair.Key] = pair.Value;
            }
        }

        private int GetInput() => Inputs.Dequeue();

        private void SetOutput(int value) => Outputs.Enqueue(value);

        /// <summary>
        /// Adds the given input and runs the computer until an output occurs.
        /// Returns the first output or null, if the comuter halts.
        /// </summary>
        public int? RunWith(int input)
        {
            Inputs.Enqueue(input);
            while (CurrentOpcode != OpCode.Halt)
            {
                ExecuteStep();
                if (CurrentOpcode == OpCode.Store)
                    return Outputs.Dequeue();
            }
            return null;
        }

        public void Run(int maxSteps = int.MaxValue)
        {
            while (maxSteps-- > 0 && CurrentOpcode != OpCode.Halt)
                ExecuteStep();
        }

        private void ExecuteStep()
        {
            StepCount++;
            _currentInstruction = GetInstruction();

            switch (_currentInstruction.OpCode)
            {
                case OpCode.Add:
                    ExecuteInstruction(_currentInstruction, (a, b) => a + b);
                    break;
                case OpCode.Mul:
                    ExecuteInstruction(_currentInstruction, (a, b) => a * b);
                    break;
                case OpCode.Load:
                    ExecuteInstruction(GetInput);
                    break;
                case OpCode.Store:
                    ExecuteInstruction(_currentInstruction, a => SetOutput(a));
                    break;

                case OpCode.JmpIfTrue:
                    _ = EvaluateInstruction(_currentInstruction, (a, b) => a != 0 ? InstructionPointer = b : 0);
                    break;
                case OpCode.JmpIfFalse:
                    _ = EvaluateInstruction(_currentInstruction, (a, b) => a == 0 ? InstructionPointer = b : 0);
                    break;

                case OpCode.LessThan:
                    ExecuteInstruction(_currentInstruction, (a, b) => a < b ? 1 : 0);
                    break;
                case OpCode.Equals:
                    ExecuteInstruction(_currentInstruction, (a, b) => a == b ? 1 : 0);
                    break;

                case OpCode.SetRelativeBase:
                    ExecuteInstruction(_currentInstruction, a => _relativeBase += a);
                    break;

                case OpCode.Halt:
                    return;
                default:
                    throw new InvalidOperationException("Unknown opcode: " + _currentInstruction.OpCode);
            }
        }


        private int GetNextArg(Instruction instruction)
        {
            var paramIndex = InstructionPointer - (instruction.Location + 1);
            var argument = _memory.GetOrAdd(InstructionPointer++, 0);

            return (instruction.ParameterModes[paramIndex]) switch
            {
                ParameterMode.PositionMode => _memory.GetOrAdd(argument, 0),
                ParameterMode.ImmediateMode => argument,
                ParameterMode.RelativeMode => _memory.GetOrAdd(_relativeBase + argument, 0),
                _ => throw new InvalidOperationException("Unknown parameter mode: " + instruction.ParameterModes[paramIndex]),
            };
        }

        private void ExecuteInstruction(Instruction instruction, Func<int, int, int> action)
        {
            var result = EvaluateInstruction(instruction, action);
            var location = _memory.GetOrAdd(InstructionPointer++, 0);
            _memory[location] = result;
        }

        private int EvaluateInstruction(Instruction instruction, Func<int, int, int> action)
            => action(GetNextArg(instruction), GetNextArg(instruction));

        private void ExecuteInstruction(Func<int> action)
        {
            var location = _memory.GetOrAdd(InstructionPointer++, 0);
            _memory[location] = action();
        }

        private void ExecuteInstruction(Instruction instruction, Action<int> action)
            => action(GetNextArg(instruction));


        private Instruction GetInstruction() => new Instruction(_memory[InstructionPointer], InstructionPointer++);

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
            ImmediateMode = 1,
            RelativeMode = 2
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
            SetRelativeBase = 9,
            Halt = 99
        }
    }
}
