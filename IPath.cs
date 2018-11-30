namespace Core
{
	public interface IPath<out TNode, out TEdge>
	{
		TNode Target { get; }
		int Length { get; }
		TNode[] Steps { get; }
	}
}