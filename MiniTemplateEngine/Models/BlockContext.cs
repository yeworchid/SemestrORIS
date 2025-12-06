namespace MiniTemplateEngine.Utils;

enum BlockType { If, Else, For }

class BlockContext
{
    public BlockType Type;
    public bool ConditionResult;
    public List<string> Body = new();
}
