/// <reference path="../../../node_modules/@types/knockout/index.d.ts" />
namespace App {
	export enum Status {
		Striving = 5,
		Endangered,
		Instinct
	}

	export abstract class Animal {
		constructor(model?: any) {
			this.name = ko.observable((model && model.hasOwnProperty('name'))? model.name : null);
			this.legs = ko.observable((model && model.hasOwnProperty('legs'))? model.legs : null);
			this.status = ko.observable((model && model.hasOwnProperty('status'))? model.status : null);
		}

		public copy(model: any) {
			this.name((model && model.hasOwnProperty('name'))? model.name : null);
			this.legs((model && model.hasOwnProperty('legs'))? model.legs : null);
			this.status((model && model.hasOwnProperty('status'))? model.status : null);
		}

		name: KnockoutObservable<string>;
		legs: KnockoutObservable<number>;
		status: KnockoutObservable<Status>;
	}

	export abstract class Feline implements Animal {
		constructor(model?: any) {
			this.whiskers = ko.observable((model && model.hasOwnProperty('whiskers'))? model.whiskers : null);
		}

		public copy(model: any) {
			this.whiskers((model && model.hasOwnProperty('whiskers'))? model.whiskers : null);
		}

		whiskers: KnockoutObservable<number>;
	}

	export abstract class Pathera implements Animal {
		constructor(model?: any) {
			this.stealth = ko.observable((model && model.hasOwnProperty('stealth'))? model.stealth : null);
		}

		public copy(model: any) {
			this.stealth((model && model.hasOwnProperty('stealth'))? model.stealth : null);
		}

		stealth: KnockoutObservable<number>;
	}

	export abstract class Lion implements Feline, extends Pathera {
		constructor(model?: any) {
			this.kills = ko.observable((model && model.hasOwnProperty('kills'))? model.kills : null);
		}

		public copy(model: any) {
			this.kills((model && model.hasOwnProperty('kills'))? model.kills : null);
		}

		kills: KnockoutObservable<number>;
	}
}
