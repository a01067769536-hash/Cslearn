using System;
using System.Collections.Generic;

// --- 최상위 문 (Top-Level Statements) 게임 루프 ---
WaveManager.InitWave();

while (true)
{
    // 1. 렌더링 (Render)
    Render();

    // 2. 입력 (Input)
    ConsoleKey key = Console.ReadKey(true).Key;
    if (key == ConsoleKey.Escape)
    {
        Console.WriteLine("\n게임을 종료합니다.");
        break;
    }

    bool isClicked = (key == ConsoleKey.Spacebar || key == ConsoleKey.Enter);

    // 3. 업데이트 (Update)
    Update(isClicked);
}

static void Update(bool isClicked)
{
    if (isClicked)
    {
        WaveManager.DamageCurrentMonster(10);
    }

    WaveManager.UpdateWave();
}

static void Render()
{
    Console.Clear();
    Console.WriteLine("==========================================");
    Console.WriteLine($" [현재 웨이브: {WaveManager.CurrentWave}]  남은 몬스터: {WaveManager.AliveMonsterCount} / {WaveManager.TotalMonstersInWave}");
    Console.WriteLine("==========================================");

    Monster? target = WaveManager.GetFrontMonster();
    if (target != null)
    {
        Console.WriteLine($"\n ▶ 타겟 몬스터: {target.Name}");
        Console.WriteLine($" ▶ 체력(HP)   : {target.Hp} / {target.MaxHp}");
    }

    Console.WriteLine("\n------------------------------------------");
    Console.WriteLine(" [Space / Enter] 클릭(공격 데미지 10) | [ESC] 종료");
}

// --- 클래스 및 정적 매니저 정의 ---
public class Monster
{
    public string Name { get; }
    public int MaxHp { get; }
    public int Hp { get; private set; }
    public bool IsDead => Hp <= 0;

    public Monster(string name, int maxHp)
    {
        Name = name;
        MaxHp = maxHp;
        Hp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        Hp = Math.Max(0, Hp - damage);
    }
}

public static class WaveManager
{
    private static List<Monster> _monsters = new();

    public static int CurrentWave { get; private set; } = 1;
    public static int TotalMonstersInWave { get; private set; }
    public static int AliveMonsterCount => _monsters.Count;

    public static void InitWave()
    {
        _monsters.Clear();
        // 웨이브에 따라 몬스터 수(n마리) 증가 (예: 1웨이브=3마리, 2웨이브=4마리...)
        int spawnCount = 2 + CurrentWave;
        TotalMonstersInWave = spawnCount;

        for (int i = 1; i <= spawnCount; i++)
        {
            int monsterHp = 20 + (CurrentWave * 10);
            _monsters.Add(new Monster($"슬라임 Lv.{CurrentWave} #{i}", monsterHp));
        }
    }

    public static Monster? GetFrontMonster()
    {
        return _monsters.Count > 0 ? _monsters[0] : null;
    }

    public static void DamageCurrentMonster(int damage)
    {
        Monster? target = GetFrontMonster();
        if (target == null) return;

        target.TakeDamage(damage);

        if (target.IsDead)
        {
            _monsters.RemoveAt(0);
        }
    }

    public static void UpdateWave()
    {
        // 현재 웨이브의 n마리 몬스터가 전부 죽으면 다음 웨이브로 전환
        if (_monsters.Count == 0)
        {
            CurrentWave++;
            InitWave();
        }
    }
}
