Enemy e1 = new Enemy("고블린");
Enemy e2 = new Enemy("고블린");
Enemy e3 = new Enemy("고블린");
class Enemy
{
    public static int Count;   // 클래스에 하나
    public string Name;        // 객체마다 하나

    public Enemy(string name)
    {
        Name = name;
        Count++;               // 모두 같은 Count
    }
    public static void PrintCount()
    {
        Console.WriteLine($"적 {Count}마리");
    }
}