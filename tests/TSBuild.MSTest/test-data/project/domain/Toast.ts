namespace TSMin {
    export class Toast {
        public static toMoney(value: number): string {
            return `$${value}`;
        }

        public static debug(message: string): void {
            console.log("debug: " + message);
        }

        public static info(message: string): void {
            let ele = document.getElementById("msg");
            ele.innerHTML = `${message}`;
        }
    }
}