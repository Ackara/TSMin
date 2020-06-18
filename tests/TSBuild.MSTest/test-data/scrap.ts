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

	export abstract class PersonBase {
		constructor() {
		}

		public id: number;
		public name: string;
	}
}
