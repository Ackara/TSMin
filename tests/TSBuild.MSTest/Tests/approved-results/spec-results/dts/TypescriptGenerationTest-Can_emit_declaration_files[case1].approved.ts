interface IBasicInfo {
	firstName?: string;
	lastName?: string;
	address?: string;
	state?: string;
	city?: string;
	zipcode?: string;
}

interface Form720 extends IBasicInfo {
	gross?: number;
	payments?: number;
	firstName?: string;
	lastName?: string;
	address?: string;
	state?: string;
	city?: string;
	zipcode?: string;
}
