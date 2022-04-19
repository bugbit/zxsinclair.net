namespace generateZ80Ops
{
    public struct OpCodeArgs
    {
        public string? Id { get; init; }
        public string? HexCode { get; init; }
        public string? OpCode { get; init; }
        public string[] Params { get; init; }
    }
}