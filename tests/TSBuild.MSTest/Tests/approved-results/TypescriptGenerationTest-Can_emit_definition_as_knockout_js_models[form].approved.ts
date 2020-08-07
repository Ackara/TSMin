/// <reference path="../../../node_modules/@types/knockout/index.d.ts" />

namespace App {
	export abstract class LineItem {
		constructor(model?: any) {
			this.position = ko.observable((model && model.hasOwnProperty('position'))? model.position : null);
			this.name = ko.observable((model && model.hasOwnProperty('name'))? model.name : null);
		}

		public copy(model: any) {
			this.position((model && model.hasOwnProperty('position'))? model.position : null);
			this.name((model && model.hasOwnProperty('name'))? model.name : null);
		}

		position: KnockoutObservable<number>;
		name: KnockoutObservable<string>;
	}

	export abstract class Form {
		constructor(model?: any) {
			this.name = ko.observable((model && model.hasOwnProperty('name'))? model.name : null);
			this.items = ko.observableArray((model && model.hasOwnProperty('items'))? model.items : null);
		}

		public copy(model: any) {
			this.name((model && model.hasOwnProperty('name'))? model.name : null);
			this.items((model && model.hasOwnProperty('items'))? model.items : null);
		}

		name: KnockoutObservable<string>;
		items: KnockoutObservableArray<LineItem>;
	}
}
