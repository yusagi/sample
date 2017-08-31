


enum Layer
{
    PLAYER = 8,
    PLANET = 9,
    PILLER = 10,
    ENEMY  = 11,
};

enum LayerMask
{
    PLAYER = 1 << 8,
    PLANET = 1 << 9,
    PILLER = 1 << 10,
    ENEMY  = 1 << 11,
};