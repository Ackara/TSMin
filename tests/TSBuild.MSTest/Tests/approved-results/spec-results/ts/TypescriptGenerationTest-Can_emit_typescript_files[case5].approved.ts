namespace App {
	export class Book {
		id: string;
		titel: string;
	}

	export class Library {
		books: Array<Book>;
		selectedBook: Book;
		coupons: Array<string>;
		featuredBooks: Array<Book>;
	}
}
