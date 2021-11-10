import pygame as pg
from math import floor, ceil


pg.font.init()


WIN_WIDTH, WIN_HEIGHT = 1080, 720
BLOCK_SIZE = 40
GRID_SIZE = 16
GRID_START = ((WIN_WIDTH - BLOCK_SIZE * GRID_SIZE) / 2, (WIN_HEIGHT - BLOCK_SIZE * GRID_SIZE) / 2)
PLAYER_SPEED = .01

BACKGROUND_COLOR = (50, 50, 50)
GRID_COLOR = (150, 150, 150)
SELECTION_COLOR = (75, 75, 75)
WALL_SELECTION_COLOR = (90, 90, 125)
WALL_COLOR = (100, 100, 150)
TEXT_COLOR = (255, 255, 255)
PLAYER_COLOR = (0, 0, 255)
AABB_COLOR = (255, 0, 0)
LINE_HEIGHT = 24
LINE_WIDTH = 2

EMPTY = 0
WALL = 1

FONT = pg.font.SysFont("Arial", 24)


def to_screen_coords(grid_coords):
    return int(GRID_START[0] + grid_coords[0] * BLOCK_SIZE), \
           int(GRID_START[1] + grid_coords[1] * BLOCK_SIZE)


def main():
    screen = pg.display.set_mode((WIN_WIDTH, WIN_HEIGHT))
    pg.display.set_caption("Lighting")

    def text(line, value):
        screen.blit(
            FONT.render(value, True, TEXT_COLOR),
            (10, 10 + line * LINE_HEIGHT)
        )

    def block(coords, color):
        screen_coords = to_screen_coords(coords)
        pg.draw.rect(
            screen, color,
            pg.Rect(
                screen_coords[0],
                screen_coords[1],
                BLOCK_SIZE, BLOCK_SIZE
            )
        )

    light = [[1.0 for y in range(GRID_SIZE)] for x in range(GRID_SIZE)]
    grid = [[EMPTY for y in range(GRID_SIZE)] for x in range(GRID_SIZE)]

    def update_light():
        for y in range(0, GRID_SIZE):
            for x in range(0, GRID_SIZE):
                light[x][y] = 1.0

        for y in range(0, GRID_SIZE):
            for x in range(0, GRID_SIZE):
                if grid[x][y] == WALL:
                    for y_ in range(y + 1, GRID_SIZE):
                        light[x][y_] = 0.0

        for i in range(10):
            for y in range(0, GRID_SIZE):
                for x in range(0, GRID_SIZE):
                    if grid[x][y] == EMPTY:
                        for dx in range(-1, 2):
                            for dy in range(-1, 2):
                                if (dx == 0 or dy == 0 and not (dx + dy == 0)) \
                                    and 0 <= x + dx < GRID_SIZE and 0 <= y + dy < GRID_SIZE \
                                    and grid[x + dx][y + dy] == EMPTY:
                                    light[x][y] = max(light[x][y], light[x + dx][y + dy] - 0.1)


    while True:
        for e in pg.event.get():
            if e.type == pg.QUIT:
                return

        screen.fill(BACKGROUND_COLOR)

        mouse_real_x, mouse_real_y = pg.mouse.get_pos()
        sel_x = floor((mouse_real_x - GRID_START[0]) / BLOCK_SIZE)
        sel_y = floor((mouse_real_y - GRID_START[1]) / BLOCK_SIZE)

        if 0 <= sel_x < GRID_SIZE and 0 <= sel_y < GRID_SIZE:
            pressed_buttons = pg.mouse.get_pressed()
            if pressed_buttons[0]:
                grid[sel_x][sel_y] = EMPTY
            elif pressed_buttons[2]:
                grid[sel_x][sel_y] = WALL

            update_light()

        # Draw selection and walls

        for x in range(0, GRID_SIZE):
            for y in range(0, GRID_SIZE):
                if grid[x][y] == WALL:
                    block((x, y), WALL_COLOR)
                else:
                    col = 255 * light[x][y]
                    block((x, y), (col, col, col))

        if 0 <= sel_x < GRID_SIZE and 0 <= sel_y < GRID_SIZE:
            block((sel_x, sel_y), SELECTION_COLOR if grid[sel_x][sel_y] == EMPTY else WALL_SELECTION_COLOR)

        # Draw grid

        for x in range(0, GRID_SIZE + 1):
            pg.draw.line(
                screen, GRID_COLOR,
                to_screen_coords((x, 0)),
                to_screen_coords((x, GRID_SIZE))
            )

            pg.draw.line(
                screen, GRID_COLOR,
                to_screen_coords((0, x)),
                to_screen_coords((GRID_SIZE, x))
            )

        pg.display.flip()


if __name__ == "__main__":
    main()