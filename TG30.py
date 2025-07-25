import time
from machine import UART, Pin

led = Pin("LED", Pin.OUT)

# TF-Luna UART on GP4 (TX1) and GP5 (RX1)
uart_luna = UART(1, baudrate=115200, tx=Pin(4), rx=Pin(5))

# Debug / output UART on GP0 (TX0) and GP1 (RX0)
uart_debug = UART(0, baudrate=115200, tx=Pin(0), rx=Pin(1))

# Button on GP28 with pull-up
button = Pin(28, Pin.IN, Pin.PULL_UP)

last_print = time.ticks_ms()

def read_frame():
    while uart_luna.any() >= 9:
        b = uart_luna.read(9)
        if b and len(b) >= 9:
            for i in range(len(b)-1):
                if b[i] == 0x59 and b[i+1] == 0x59:
                    distance = b[i+2] | (b[i+3] << 8)
                    strength = b[i+4] | (b[i+5] << 8)
                    return distance, strength
    return None, None

print("=== Pico TF-Luna Reader started ===")

while True:
    # 🔘 Check button state (0 = pressed because pull-up)
    if button.value() == 0:
        # Send "102" once per press (optional debounce)
        uart_debug.write("102\r\n")
        # Simple debounce delay so it doesn't spam if held
        time.sleep(0.2)

    # 📡 Read TF-Luna
    d, s = read_frame()
    if d is not None:
        if time.ticks_diff(time.ticks_ms(), last_print) >= 100:
            uart_debug.write(f"{d},{s}\r\n")
            last_print = time.ticks_ms()
            led.toggle()

    time.sleep(0.01)

