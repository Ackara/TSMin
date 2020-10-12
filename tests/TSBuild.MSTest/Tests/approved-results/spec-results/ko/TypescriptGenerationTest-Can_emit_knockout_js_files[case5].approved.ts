/// <reference path="../../../../node_modules/@types/knockout/index.d.ts" />

namespace App {
	export class BookModel {
		constructor(model?: any) {
			this.id = ko.observable((model && model.hasOwnProperty('id'))? model.id : null);
			this.titel = ko.observable((model && model.hasOwnProperty('titel'))? model.titel : null);
		}

		public copy(model: any) {
			this.id((model && model.hasOwnProperty('id'))? model.id : null);
			this.titel((model && model.hasOwnProperty('titel'))? model.titel : null);
		}

		id: KnockoutObservable<string>;
		titel: KnockoutObservable<string>;
	}

	export class LibraryModel {
		constructor(model?: any) {
			this.books = ko.observableArray((model && model.hasOwnProperty('books'))? model.books : null);
			this.selectedBook = new BookModel((model && model.hasOwnProperty('selectedBook'))? model.selectedBook : null);
		}

		public copy(model: any) {
			this.books((model && model.hasOwnProperty('books'))? model.books : null);
			this.selectedBook.copy((model && model.hasOwnProperty('selectedBook'))? model.selectedBook : null);
		}

		books: KnockoutObservableArray<BookModel>;
		selectedBook: BookModel;
	}
}
