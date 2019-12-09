using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class LongCodeComputer
    {
        public long[] Memory => _memory.Values.ToArray();
        public Dictionary<int, long> _memory { get; private set; }

        public int InstructionPointer { get; private set; }
        public int StepCount { get; private set; }

        public Queue<long> Inputs { get; } = new Queue<long>();
        public Queue<long> Outputs { get; } = new Queue<long>();

        private Instruction _currentInstruction;
        private int _relativeBase = 0;

        public OpCode CurrentOpcode => _currentInstruction.OpCode;

        public LongCodeComputer(long[] initialState)
        {
            _memory = initialState.Select((a, i) => (data: a, index: i))
                .ToDictionary(x => x.index, x => x.data);
        }

        public LongCodeComputer(long[] initialState, long firstInput) : this(initialState)
        {
            Inputs.Enqueue(firstInput);
        }

        public LongCodeComputer(long[] initialState, IDictionary<int, long> stateOverride) : this(initialState)
        {
            foreach (var pair in stateOverride)
            {
                _memory[pair.Key] = pair.Value;
            }
        }

        private long GetInput() => Inputs.Dequeue();

        private void SetOutput(long value) => Outputs.Enqueue(value);

        /// <summary>
        /// Adds the given input and runs the computer until an output occurs.
        /// Returns the first output or null, if the comuter halts.
        /// </summary>
        public long? RunWith(long input)
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
                    ExecuteInstruction(_currentInstruction, (a, b) => checked(a + b));
                    break;
                case OpCode.Mul:
                    ExecuteInstruction(_currentInstruction, (a, b) => checked(a * b));
                    break;
                case OpCode.Load:
                    ExecuteInstruction(GetInput);
                    break;
                case OpCode.Store:
                    ExecuteInstruction(_currentInstruction, a => SetOutput(a));
                    break;

                case OpCode.JmpIfTrue:
                    _ = EvaluateInstruction(_currentInstruction, (a, b) => a != 0 ? InstructionPointer = (int)b : 0);
                    break;
                case OpCode.JmpIfFalse:
                    _ = EvaluateInstruction(_currentInstruction, (a, b) => a == 0 ? InstructionPointer = (int)b : 0);
                    break;

                case OpCode.LessThan:
                    ExecuteInstruction(_currentInstruction, (a, b) => a < b ? 1 : 0);
                    break;
                case OpCode.Equals:
                    ExecuteInstruction(_currentInstruction, (a, b) => a == b ? 1 : 0);
                    break;

                case OpCode.SetRelativeBase:
                    ExecuteInstruction(_currentInstruction, a => _relativeBase = (int)(_relativeBase + a));
                    break;

                case OpCode.Halt:
                    return;
                default:
                    throw new InvalidOperationException("Unknown opcode: " + _currentInstruction.OpCode);
            }
        }


        private long GetNextArg(Instruction instruction)
        {
            var paramIndex = InstructionPointer - (instruction.Location + 1);
            var argument = (int)_memory.GetOrAdd(InstructionPointer++, 0);

            return (instruction.ParameterModes[paramIndex]) switch
            {
                ParameterMode.PositionMode => _memory.GetOrAdd(argument, 0),
                ParameterMode.ImmediateMode => argument,
                ParameterMode.RelativeMode => _memory.GetOrAdd(_relativeBase + argument, 0),
                _ => throw new InvalidOperationException("Unknown parameter mode: " + instruction.ParameterModes[paramIndex]),
            };
        }
        private int GetNextAddr(Instruction instruction)
        {
            var paramIndex = InstructionPointer - (instruction.Location + 1);
            var argument = (int)_memory.GetOrAdd(InstructionPointer++, 0);

            return (instruction.ParameterModes[paramIndex]) switch
            {
                ParameterMode.PositionMode => argument,
                ParameterMode.RelativeMode => _relativeBase + argument,
                _ => throw new InvalidOperationException("Invalid or unknown parameter mode: " + instruction.ParameterModes[paramIndex]),
            };
        }

        private void ExecuteInstruction(Instruction instruction, Func<long, long, long> action)
        {
            var result = EvaluateInstruction(instruction, action);
            var resultAddress = GetNextAddr(_currentInstruction);
            _memory[resultAddress] = result;
        }

        private long EvaluateInstruction(Instruction instruction, Func<long, long, long> action)
            => action(GetNextArg(instruction), GetNextArg(instruction));

        private void ExecuteInstruction(Func<long> action)
        {
            var resultAddress = GetNextAddr(_currentInstruction);
            _memory[resultAddress] = action();
        }

        private void ExecuteInstruction(Instruction instruction, Action<long> action)
            => action(GetNextArg(instruction));


        private Instruction GetInstruction() => new Instruction(_memory[InstructionPointer], InstructionPointer++);

        private struct Instruction
        {
            public OpCode OpCode { get; }
            public readonly ParameterMode[] ParameterModes;

            public readonly int Location { get; }

            public Instruction(long instructionCode, int location)
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