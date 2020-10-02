/// <reference path="../../../../node_modules/@types/knockout/index.d.ts" />

namespace App {
	export class AnimalModel {
		constructor(model?: any) {
			this.name = ko.observable((model && model.hasOwnProperty('name'))? model.name : null);
			this.hitPoints = ko.observable((model && model.hasOwnProperty('hitPoints'))? model.hitPoints : null);
		}

		public copy(model: any) {
			this.name((model && model.hasOwnProperty('name'))? model.name : null);
			this.hitPoints((model && model.hasOwnProperty('hitPoints'))? model.hitPoints : null);
		}

		name: KnockoutObservable<string>;
		hitPoints: KnockoutObservable<number>;
	}

	export class TigerModel extends AnimalModel {
		constructor(model?: any) {
			super(model);
			this.hasFangs = ko.observable((model && model.hasOwnProperty('hasFangs'))? model.hasFangs : null);
		}

		public copy(model: any) {
			this.hasFangs((model && model.hasOwnProperty('hasFangs'))? model.hasFangs : null);
		}

		hasFangs: KnockoutObservable<boolean>;
	}
}
