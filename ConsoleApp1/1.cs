
class block
{
    public string shape = "ㅁ";

    public block(string shape)
    {
        this.shape = shape;
    }
}

class TimerBlock : block
{
    public int DeathTimer = 3;
    public TimerBlock(string shape) : base(shape)
    {
    }
    public void TimeDeath()
    {

    }
}