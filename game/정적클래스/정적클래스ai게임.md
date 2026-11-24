- [몬스터](basic/Monster.cs)
- [프로그램](basic/Program.cs)
- [웨이브메니저](basic/WaveManager.cs)

정적 클래스 활요을 찾아서 아래를 작성하세요
## what
몬스터 웨이브 시스템이 살아있는 몬스터 수를 정적 클래스와 변수를 활용해서 구현

## how
ai 해석 및 활용을 위해서 어덯게 하면 클래스 안의 코드를 줄일 수 있을까?

## 정리

게입을 만들어라(x)
입력 업데이트 렌더링 구현
디테일 주기

```
public static class WaveManager
    {
        private static readonly List<Monster> _monsters = new List<Monster>();

        public static int CurrentWave { get; private set; } = 1;
        public static int TotalMonsterCount => _monsters.Count;
        public static int RemainingMonsterCount => _monsters.Count(m => !m.IsDead);
        ```