/// <reference path="Observable.ts" />

namespace TSMin {
    export class Car extends Observable {
        constructor() {
            super();

            this.make = "Forte";
            this.year = 2019;
        }

        public make: string;
        public year: number;
    }
}