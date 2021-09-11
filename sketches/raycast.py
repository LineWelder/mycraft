import pygame as pg
from math import floor, ceil


pg.font.init()


WIN_WIDTH, WIN_HEIGHT = 1920, 1080
BLOCK_SIZE = 60
GRID_SIZE = 16
GRID_START = ((WIN_WIDTH - BLOCK_SIZE * GRID_SIZE) / 2, (WIN_HEIGHT - BLOCK_SIZE * GRID_SIZE) / 2)

BACKGROUND_COLOR = (50, 50, 50)
GRID_COLOR = (150, 150, 150)
SELECTION_COLOR = (75, 75, 75)
WALL_SELECTION_COLOR = (125, 125, 125)
WALL_COLOR = (150, 150, 150)
RAY_COLOR = (255, 255, 255)
RAY_ORIGIN_COLOR = (255, 50, 50)
RAY_DIRECTION_COLOR = (50, 255, 50)
TEXT_COLOR = (255, 255, 255)
LINE_HEIGHT = 24
POINT_RADIUS = 5

EMPTY = 0
WALL = 1

EDITING_WALLS_STATE = 0
EDITING_ORIGIN_STATE = 1
EDITING_DIRECTION_STATE = 2

FONT = pg.font.SysFont("Arial", 24)


def to_screen_coords(grid_coords):
    return int(GRID_START[0] + grid_coords[0] * BLOCK_SIZE), \
           int(GRID_START[1] + grid_coords[1] * BLOCK_SIZE)


# a will not be 0
def sign(a):
    return 1 if a >= 0 else -1


def sqr_distance(a, b):
    dx = b[0] - a[0]
    dy = b[1] - a[1]
    return dx * dx + dy * dy


def main():
    screen = pg.display.set_mode((1920, 1080))
    pg.display.set_caption("Ray casting")

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

    def raycast(origin, direction, max_distance):
        # Since we use squared distance, squaring the max distance parameter
        max_distance *= max_distance

        # If ray is aligned with one of the axis, it will not go through the faces parallel to it
        check_horizontal = direction[1] != 0
        check_vertical = direction[0] != 0

        # If direction vector is zero, we cannot cast the ray
        if not check_vertical and not check_horizontal:
            raise Exception("direction must not be zero")

        # The step we move our horizontal face checker
        if check_horizontal:
            check_horizontal_step = (
                sign(direction[1]) * direction[0] / direction[1],
                sign(direction[1])
            )

        # The step we move our vertical faces checker
        if check_vertical:
            check_vertical_step = (
                sign(direction[0]),
                sign(direction[0]) * direction[1] / direction[0]
            )

        # The face checkers
        current_horizontal = origin
        current_vertical = origin

        # Move the face checkers to the fist face on their way
        if check_vertical:
            x = ceil(origin[0]) if direction[0] >= 0 else floor(origin[0])
            current_vertical = (x, origin[1] + abs(x - origin[0]) * check_vertical_step[1])

        if check_horizontal:
            y = ceil(origin[1]) if direction[1] >= 0 else floor(origin[1])
            current_horizontal = (origin[0] + abs(y - origin[1]) * check_horizontal_step[0], y)

        while True:
            distance_to_horizontal = sqr_distance(origin, current_horizontal)
            distanct_to_vertical = sqr_distance(origin, current_vertical)
            next_is_horizontal = check_horizontal \
                            and (not check_vertical or distance_to_horizontal < distanct_to_vertical)

            if next_is_horizontal:
                pg.draw.circle(screen, RAY_COLOR, to_screen_coords(current_horizontal), POINT_RADIUS // 2)

                if distance_to_horizontal > max_distance:
                    return False
                
                hit_block = (
                    floor(current_horizontal[0]),
                    current_horizontal[1] - (1 if check_horizontal_step[1] < 0 else 0)
                )

                current_horizontal = (
                    current_horizontal[0] + check_horizontal_step[0],
                    current_horizontal[1] + check_horizontal_step[1]
                )
            else:
                pg.draw.circle(screen, RAY_COLOR, to_screen_coords(current_vertical), POINT_RADIUS // 2)

                if distanct_to_vertical > max_distance:
                    return False

                hit_block = (
                    current_vertical[0] - (1 if check_vertical_step[0] < 0 else 0),
                    floor(current_vertical[1])
                )

                current_vertical = (
                    current_vertical[0] + check_vertical_step[0],
                    current_vertical[1] + check_vertical_step[1]
                )

            if 0 <= hit_block[0] < GRID_SIZE and 0 <= hit_block[1] < GRID_SIZE \
               and grid[hit_block[0]][hit_block[1]] == WALL:
                block(hit_block, SELECTION_COLOR)
                return True

    grid = [[EMPTY for y in range(GRID_SIZE)] for x in range(GRID_SIZE)]
    state = EDITING_WALLS_STATE
    ray_origin = (0, 0)
    ray_direction = (0, 0)

    while True:
        for e in pg.event.get():
            if e.type == pg.QUIT:
                return
            elif e.type == pg.KEYDOWN:
                if e.key == pg.K_1:
                    state = EDITING_WALLS_STATE
                elif e.key == pg.K_2:
                    state = EDITING_ORIGIN_STATE
                elif e.key == pg.K_3:
                    state = EDITING_DIRECTION_STATE

        screen.fill(BACKGROUND_COLOR)

        mouse_real_x, mouse_real_y = pg.mouse.get_pos()
        mouse_x = (mouse_real_x - GRID_START[0]) / BLOCK_SIZE
        mouse_y = (mouse_real_y - GRID_START[1]) / BLOCK_SIZE
        sel_x = floor(mouse_x)
        sel_y = floor(mouse_y)

        if state == EDITING_WALLS_STATE:
            pressed_buttons = pg.mouse.get_pressed()
            if pressed_buttons[0]:
                grid[sel_x][sel_y] = EMPTY
            elif pressed_buttons[2]:
                grid[sel_x][sel_y] = WALL

        elif state == EDITING_ORIGIN_STATE:
            ray_origin = (mouse_x, mouse_y)

        elif state == EDITING_DIRECTION_STATE:
            ray_direction = (mouse_x, mouse_y) 

        # Draw selection and walls

        for x in range(0, GRID_SIZE):
            for y in range(0, GRID_SIZE):
                if grid[x][y] == WALL:
                    block((x, y), WALL_COLOR)

        if state == EDITING_WALLS_STATE:
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

        # Draw ray related stuff

        if state >= EDITING_DIRECTION_STATE:
            pg.draw.line(screen, RAY_COLOR, to_screen_coords(ray_origin), to_screen_coords(ray_direction))

        if state >= EDITING_ORIGIN_STATE:
            pg.draw.circle(screen, RAY_ORIGIN_COLOR, to_screen_coords(ray_origin), POINT_RADIUS)

        if state >= EDITING_DIRECTION_STATE:
            pg.draw.circle(screen, RAY_DIRECTION_COLOR, to_screen_coords(ray_direction), POINT_RADIUS)

            if ray_direction != ray_origin:
                text(4, f"Raycast result: {raycast(ray_origin, (ray_direction[0] - ray_origin[0], ray_direction[1] - ray_origin[1]), 16)}")

        # Draw text

        text(0, "Press 1 to edit walls, 2 to edit ray origin, 3 to edit ray direction")
        state_text = "Editing walls" if state == EDITING_WALLS_STATE \
                else "Editing ray origin" if state == EDITING_ORIGIN_STATE \
                else "Editing ray direction"
        text(1, f"Current state: {state_text}")

        if state == EDITING_WALLS_STATE:
            selection_text = f"({sel_x}, {sel_y})" if 0 <= sel_x < GRID_SIZE \
                                                  and 0 <= sel_y < GRID_SIZE \
                        else "None"
            text(2, f"Selection: {selection_text}")

        if state >= EDITING_ORIGIN_STATE:
            text(2, f"Ray origin: ({ray_origin[0]:.2f}, {ray_origin[1]:.2f})")

        if state >= EDITING_DIRECTION_STATE:
            text(3, f"Ray direction: ({ray_direction[0]:.2f}, {ray_direction[1]:.2f})")

        pg.display.flip()


if __name__ == "__main__":
    main()