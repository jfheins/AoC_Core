namespace Core
{
	public interface IPath<out TNode>
	{
		TNode Target { get; }
		int Length { get; }
		TNode[] Steps { get; }
	}
}