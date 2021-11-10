import pygame as pg
from math import floor, ceil


pg.font.init()


WIN_WIDTH, WIN_HEIGHT = 1080, 720
BLOCK_SIZE = 40
GRID_SIZE = 16
GRID_START = ((WIN_WIDTH - BLOCK_SIZE * GRID_SIZE) / 2, (WIN_HEIGHT - BLOCK_SIZE * GRID_SIZE) / 2)
PLAYER_SPEED = .007

BACKGROUND_COLOR = (50, 50, 50)
GRID_COLOR = (150, 150, 150)
SELECTION_COLOR = (75, 75, 75)
WALL_SELECTION_COLOR = (125, 125, 125)
WALL_COLOR = (150, 150, 150)
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


# Not perfect, but works pretty fine
class AABB:
    def __init__(self, pos, size):
        self.pos = pos
        self.size = size
        self.last_pos = pos
        self.delta = (0, 0)

    # Moves the box immediately
    def move(self, dx, dy):
        self.pos = (self.pos[0] + dx, self.pos[1] + dy)
        self.delta = (self.delta[0] + dx, self.delta[1] + dy)

    # But then resolves the collision moving the box along one axis at a time
    def collide(self, others):
        self.pos = self.last_pos

        self.pos = (self.pos[0], self.last_pos[1] + self.delta[1])
        for other in others:
            if self.delta[1] != 0 and other.pos[0] - self.size[0] < self.pos[0] < other.pos[0] + other.size[0]:
                if self.delta[1] > 0 and self.last_pos[1] <= other.pos[1] - self.size[1] < self.pos[1]:
                    self.pos = (self.pos[0], other.pos[1] - self.size[1])
                elif self.pos[1] < other.pos[1] + other.size[1] <= self.last_pos[1]:
                    self.pos = (self.pos[0], other.pos[1] + other.size[1])

        self.pos = (self.last_pos[0] + self.delta[0], self.pos[1])
        for other in others:
            if self.delta[0] != 0 and other.pos[1] - self.size[1] < self.pos[1] < other.pos[1] + other.size[1]:
                if self.delta[0] > 0 and self.last_pos[0] <= other.pos[0] - self.size[0] < self.pos[0]:
                    self.pos = (other.pos[0] - self.size[0], self.pos[1])
                elif self.pos[0] < other.pos[0] + other.size[0] <= self.last_pos[0]:
                    self.pos = (other.pos[0] + other.size[0], self.pos[1])

        self.last_pos = self.pos
        self.delta = (0, 0)


def main():
    screen = pg.display.set_mode((WIN_WIDTH, WIN_HEIGHT))
    pg.display.set_caption("Physics")

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

    def aabb(box, color):
        pos = to_screen_coords(box.pos)
        pg.draw.rect(
            screen, color,
            pg.Rect(
                pos[0] + LINE_WIDTH,
                pos[1] + LINE_WIDTH,
                box.size[0] * BLOCK_SIZE - LINE_WIDTH,
                box.size[1] * BLOCK_SIZE - LINE_WIDTH
            ), LINE_WIDTH
        )

    grid = [[EMPTY for y in range(GRID_SIZE)] for x in range(GRID_SIZE)]
    player = AABB((.125, .125), (.75, .75))

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

        pressed_keys = pg.key.get_pressed()

        if pressed_keys[pg.K_w]:
            player.move(0, -PLAYER_SPEED)
        elif pressed_keys[pg.K_s]:
            player.move(0, PLAYER_SPEED)

        if pressed_keys[pg.K_a]:
            player.move(-PLAYER_SPEED, 0)
        elif pressed_keys[pg.K_d]:
            player.move(PLAYER_SPEED, 0)

        aabbs = []
        
        for x in range(floor(player.pos[0]), floor(player.pos[0] + player.size[0]) + 1):
            for y in range(floor(player.pos[1]), floor(player.pos[1] + player.size[1]) + 1):
                if 0 <= x < GRID_SIZE and 0 <= y < GRID_SIZE and grid[x][y] == WALL:
                    aabbs.append(AABB((float(x), float(y)), (float(1), float(1))))

        player.collide(aabbs)

        # Draw selection and walls

        for x in range(0, GRID_SIZE):
            for y in range(0, GRID_SIZE):
                if grid[x][y] == WALL:
                    block((x, y), WALL_COLOR)

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

        # Draw AABBs

        aabb(player, PLAYER_COLOR)
        for box in aabbs:
            aabb(box, AABB_COLOR)

        pg.display.flip()


if __name__ == "__main__":
    main()