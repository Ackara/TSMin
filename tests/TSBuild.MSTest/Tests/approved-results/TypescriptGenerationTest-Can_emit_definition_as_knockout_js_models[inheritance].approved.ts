/// <reference path="../../../node_modules/@types/knockout/index.d.ts" />

namespace App {
	export enum Status {
		Striving = 5,
		Endangered,
		Instinct
	}

	export interface Animal {
		name?: string;
		legs?: number;
		status?: Status;
	}

	export interface Feline extends Animal {
		whiskers?: number;
	}

	export interface Pathera extends Animal {
		stealth?: number;
	}

	export abstract class Lion implements Feline, Pathera {
		constructor(model?: any) {
			this.name = ko.observable((model && model.hasOwnProperty('name'))? model.name : null);
			this.legs = ko.observable((model && model.hasOwnProperty('legs'))? model.legs : null);
			this.status = ko.observable((model && model.hasOwnProperty('status'))? model.status : null);
			this.kills = ko.observable((model && model.hasOwnProperty('kills'))? model.kills : null);
			this.whiskers = ko.observable((model && model.hasOwnProperty('whiskers'))? model.whiskers : null);
			this.stealth = ko.observable((model && model.hasOwnProperty('stealth'))? model.stealth : null);
		}

		public copy(model: any) {
			this.name((model && model.hasOwnProperty('name'))? model.name : null);
			this.legs((model && model.hasOwnProperty('legs'))? model.legs : null);
			this.status((model && model.hasOwnProperty('status'))? model.status : null);
			this.kills((model && model.hasOwnProperty('kills'))? model.kills : null);
			this.whiskers((model && model.hasOwnProperty('whiskers'))? model.whiskers : null);
			this.stealth((model && model.hasOwnProperty('stealth'))? model.stealth : null);
		}

		name: KnockoutObservable<string>;
		legs: KnockoutObservable<number>;
		status: KnockoutObservable<Status>;
		kills: KnockoutObservable<number>;
		whiskers: KnockoutObservable<number>;
		stealth: KnockoutObservable<number>;
	}
}
