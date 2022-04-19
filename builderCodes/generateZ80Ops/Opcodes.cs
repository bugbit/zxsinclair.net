namespace generateZ80Ops
{
    public record Opcodes
    {
        public string? FileDat { get; init; }
        public string? FileZ80EnumH { get; init; }
        public string? FileZ80OpcodesH { get; init; }
    };
}