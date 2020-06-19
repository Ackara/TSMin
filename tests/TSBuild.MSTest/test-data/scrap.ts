/// <reference path="scrap.d.ts" />

namespace TSBuild {
	export class scrap {
		constructor() {
		}

		public func(): void {
			let cat: Foo.Feline;

			if (cat.status === Foo.Status.Endangered) {
				console.debug("foo");
			}


			let simba: Foo.Lion;
		}
	}

	export enum Island {
		Stt,
		Stx,
		Stj
	}

	export interface ICommand<T> {
		execute(): T;
	}

	export abstract class PersonBase {
		constructor() {
		}

		public id: number;
		public name: string;
	}

	

	class Jeff extends PersonBase implements ICommand<any> {
		constructor() {
			super();
		}

		execute(): any {

		}
	}

	interface Foo extends PersonBase {

	}

	class Dale implements Foo {
		constructor() {
			
		}
        public id: number;
        public name: string;
	}

}
