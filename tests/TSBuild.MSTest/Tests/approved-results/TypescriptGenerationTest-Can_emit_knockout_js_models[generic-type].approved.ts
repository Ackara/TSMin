/// <reference path="../../../node_modules/@types/knockout/index.d.ts" />
namespace App {
	export abstract class Predator {
		constructor(model?: any) {
			this.name = ko.observable((model && model.hasOwnProperty('name'))? model.name : null);
		}

		public copy(model: any) {
			this.name((model && model.hasOwnProperty('name'))? model.name : null);
		}

		name: KnockoutObservable<string>;
	}

	export interface Skill<any> {
		constructor(model?: any) {
		}

		public copy(model: any) {
		}

	}

	export abstract class AfricanLion extends Predator implements Skill<any> {
		constructor(model?: any) {
			this.region = ko.observable((model && model.hasOwnProperty('region'))? model.region : null);
		}

		public copy(model: any) {
			this.region((model && model.hasOwnProperty('region'))? model.region : null);
		}

		region: KnockoutObservable<string>;
	}
}
