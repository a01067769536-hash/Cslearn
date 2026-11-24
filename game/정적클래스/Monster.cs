namespace WaveGame
{
    /// <summary>
    /// 개별 몬스터의 체력과 피격 상태를 관리하는 클래스
    /// </summary>
    public class Monster
    {
        public string Name { get; }
        public int MaxHp { get; }
        public int CurrentHp { get; private set; }
        public bool IsDead => CurrentHp <= 0;

        public Monster(string name, int maxHp)
        {
            Name = name;
            MaxHp = maxHp;
            CurrentHp = maxHp;
        }

        public void TakeDamage(int damage)
        {
            if (IsDead)
                return;

            CurrentHp -= damage;
            if (CurrentHp < 0)
            {
                CurrentHp = 0;
            }
        }
    }
}
