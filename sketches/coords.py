import pygame as pg
from math import floor, ceil


pg.font.init()


WIN_WIDTH, WIN_HEIGHT = 1920, 1080
ORIGIN = (WIN_WIDTH / 2, WIN_HEIGHT / 2)
BLOCK_SIZE = 20
CHUNK_SIZE = 16

BACKGROUND_COLOR = (50, 50, 50)
GRID_COLOR = (100, 100, 100)
CHUNK_BORDER_COLOR = (200, 200, 200)
AXIS_COLOR = (255, 255, 255)
SELECTION_COLOR = (100, 100, 50)
TEXT_COLOR = (255, 255, 255)
LINE_HEIGHT = 24

FONT = pg.font.SysFont("Arial", 24)


def get_grid_color(x):
    if x == 0:
        return AXIS_COLOR
    elif x % CHUNK_SIZE == 0:
        return CHUNK_BORDER_COLOR
    else:
        return GRID_COLOR


def csharp_mod(a, b):
    if a >= 0:
        return a % b
    else:
        return a - csharp_div(a, b) * b


def csharp_div(a, b):
    if a >= 0:
        return a // b
    else:
        return ceil(a / b)


def main():
    screen = pg.display.set_mode((1920, 1080))
    pg.display.set_caption("Coords")

    def text(line, value):
        screen.blit(
            FONT.render(value, True, TEXT_COLOR),
            (10, 10 + line * LINE_HEIGHT)
        )

    leftViewBorder = floor(-ORIGIN[0] / BLOCK_SIZE)
    rightViewBorder = ceil((WIN_WIDTH - ORIGIN[0]) / BLOCK_SIZE)
    topViewBorder = floor(-ORIGIN[1] / BLOCK_SIZE)
    bottomViewBorder = ceil((WIN_HEIGHT - ORIGIN[1]) / BLOCK_SIZE)

    while True:
        for e in pg.event.get():
            if e.type == pg.QUIT:
                return

        screen.fill(BACKGROUND_COLOR)

        mouseX, mouseY = pg.mouse.get_pos()
        selX = floor((mouseX - ORIGIN[0]) / BLOCK_SIZE)
        selZ = floor((mouseY - ORIGIN[1]) / BLOCK_SIZE)

        # Draw selection

        pg.draw.rect(
            screen, SELECTION_COLOR,
            pg.Rect(
                ORIGIN[0] + selX * BLOCK_SIZE,
                ORIGIN[1] + selZ * BLOCK_SIZE,
                BLOCK_SIZE, BLOCK_SIZE
            )
        )

        # Draw grid

        for x in range(leftViewBorder, rightViewBorder + 1):
            pg.draw.line(
                screen, get_grid_color(x),
                (ORIGIN[0] + x * BLOCK_SIZE, 0), (ORIGIN[0] + x * BLOCK_SIZE, WIN_HEIGHT)
            )

        for z in range(topViewBorder, bottomViewBorder + 1):
            pg.draw.line(
                screen, get_grid_color(z),
                (0, ORIGIN[1] + z * BLOCK_SIZE), (WIN_WIDTH, ORIGIN[1] + z * BLOCK_SIZE)
            )

        # Draw text

        divX = csharp_div(selX, CHUNK_SIZE)
        divZ = csharp_div(selZ, CHUNK_SIZE)
        modX = csharp_mod(selX, CHUNK_SIZE)
        modZ = csharp_mod(selZ, CHUNK_SIZE)

        chunkX = divX if selX >= 0 else csharp_div(selX + 1, CHUNK_SIZE) - 1
        chunkZ = divZ if selZ >= 0 else csharp_div(selZ + 1, CHUNK_SIZE) - 1
        blockX = modX if selX >= 0 else CHUNK_SIZE + csharp_mod(selX + 1, CHUNK_SIZE) - 1
        blockZ = modZ if selZ >= 0 else CHUNK_SIZE + csharp_mod(selZ + 1, CHUNK_SIZE) - 1

        text(0, f"Selected: ({selX}, {selZ})")
        text(1, f"Div: ({divX}, {divZ})")
        text(2, f"Mod: ({modX}, {modZ})")
        text(3, f"Chunk: ({chunkX}, {chunkZ})")
        text(4, f"Block: ({blockX}, {blockZ})")

        pg.display.flip()


if __name__ == "__main__":
    main()