using System.Collections.Generic;
using System.Linq;

namespace WaveGame
{
    /// <summary>
    /// 웨이브 상태 및 전체 몬스터(n마리)의 생성과 처치를 관리하는 정적 클래스
    /// </summary>
    public static class WaveManager
    {
        private static readonly List<Monster> _monsters = new List<Monster>();

        public static int CurrentWave { get; private set; } = 1;
        public static int TotalMonsterCount => _monsters.Count;
        public static int RemainingMonsterCount => _monsters.Count(m => !m.IsDead);

        /// <summary>
        /// 지정된 웨이브를 시작하고 n마리의 몬스터를 생성합니다.
        /// </summary>
        public static void StartWave(int wave)
        {
            CurrentWave = wave;
            _monsters.Clear();

            // 웨이브에 따라 몬스터 수(n)와 체력 증가
            int monsterCount = 2 + wave;
            int monsterHp = 20 + (wave - 1) * 10;

            for (int i = 1; i <= monsterCount; i++)
            {
                _monsters.Add(new Monster($"몬스터 #{i} (Wave {wave})", monsterHp));
            }
        }

        /// <summary>
        /// 현재 살아있는 첫 번째 몬스터를 반환합니다.
        /// </summary>
        public static Monster? GetCurrentMonster()
        {
            return _monsters.FirstOrDefault(m => !m.IsDead);
        }

        /// <summary>
        /// 클릭 시 현재 타겟 몬스터에게 데미지를 줍니다.
        /// </summary>
        public static void DamageCurrentMonster(int damage)
        {
            Monster? target = GetCurrentMonster();
            target?.TakeDamage(damage);
        }

        /// <summary>
        /// n마리의 몬스터가 전부 죽었으면 다음 웨이브로 넘어갑니다.
        /// </summary>
        public static void CheckAndAdvanceWave()
        {
            if (TotalMonsterCount > 0 && RemainingMonsterCount == 0)
            {
                StartWave(CurrentWave + 1);
            }
        }
    }
}
