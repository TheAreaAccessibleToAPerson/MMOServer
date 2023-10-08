public interface IUnit 
{
    void Initialize(
        string name,     // Имя
        string id,
        int lvl, int ex, // Уровень, количесво опыта. 
        uint startPositionX, uint startPositionY, // Стартовая позиция.
        uint distanceAgr, uint distanceNotArg, // Дистанция агра, дистанция разагривания.
        uint patrulLeftMaxDistance, uint patrulRightMaxDistance, // Максимальная дистанция патрулирования.
        uint patrulLeftMinDistance, uint patrulRightMinDistance, // Минимальная дистанция патрулирования.
        uint maxSitTime, uint minSitTime, // Минимальное время стояния на месте.
        uint maxDistanceFromStartPosition, // Максимальная дистанция от стартовой позиции.

        int hp, int mp, int rage, int shield, // Здоровье, мана, ярость, щит,
        int hpRegen, int mpRegen, // Реген здоровья, реген маны.
        int moveSpeed, // Скорость бега.
        int physicsAttackPower, int physicsAttackSpeed, // Сила физ атаки атаки, скорость физ атаки
        int magicDef, int physicsDef, // Магическая, физическая зацища.
        int paryPhysicsAttackChance, int paryPhysicsAttackPower, // Парирование физ атаки шанс, сила.
        int dodgePhysicsAttackChance, // Шанс уклона.
        int magicAttackPower, int magicSpeedCast, // Сила, скорость магической атаки.
        int magicResist, // Магический резист.

        int decelerationProtection, // Защита от замедления.
        int stanProtection, // Защита от стана
        int fallProtection, // Защита от опракидования.
        int departureProtection, // Защита от отравления.

        int protectionFromFireMagic, // Защита от огня.
        int protectionFromWaterMagic, // Защита от воды.
        int protectionFromWindMagic, // Защита от ветра.
        int protectionFromEarthMagic, // Защита от земли.
        int protectionFromDarkMagic, // Защита от темной магии.
        int protectionFromLightMagic, // Защита от светлой магии.

        int[] skils,
        int[] drops); // Дроп 
}