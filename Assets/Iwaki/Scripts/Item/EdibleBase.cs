public class EdibleBase : ItemBase, IEdible
{
    public void Eat()
    {
        Destroy(gameObject);
    }
}
