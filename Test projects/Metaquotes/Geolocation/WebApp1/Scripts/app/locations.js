function Location(data) {
	this.country = data.country;
	this.region = data.region;
	this.postal = data.postal;
	this.city = data.city;
	this.organization = data.organization;
	this.latitude = data.latitude;
	this.longitude = data.longitude;
}

function IpViewModel(ip, loaded, location) {
	var self = this;
	self.ip = ko.observable(ip);
	self.loaded = ko.observable(loaded);
	self.location = ko.observable(location);
	self.error = ko.observable();

	self.getLocation = function (ip) {
		if (ip && ip.length > 0) {
			window.location.hash = 'ip/' + encodeURIComponent(ip);
		}
	};
}

function CityViewModel(city, loaded, locations) {
	var self = this;
	self.city = ko.observable(city);
	self.loaded = ko.observable(loaded);
	self.locations = ko.observable(locations);
	self.error = ko.observable();

	self.getLocations = function (city) {
		if (city && city.length > 0) {
			window.location.hash = 'city/' + encodeURIComponent(city);
		}
	};
}

function LocationsViewModel() {
	var self = this;

	self.tabs = [
        { view: 'ip', title: 'Поиск по IP-адресу' },
        { view: 'city', title: 'Поиск по городу' }
	];

	self.activeTab = ko.observable();
	self.ipViewModel = ko.observable();
	self.cityViewModel = ko.observable();

	self.activateTab = function (tab) {
		window.location.hash = tab.view;
	}

	self.makeIpTabActive = function (ivm) {
		self.ipViewModel(ivm);
		self.cityViewModel(null);
		self.activeTab(self.tabs[0]);
	};

	self.makeCityTabActive = function (cvm) {
		self.ipViewModel(null);
		self.cityViewModel(cvm);
		self.activeTab(self.tabs[1]);
	};

	Sammy(function () {
		this.get('#ip', function () {
			self.makeIpTabActive(new IpViewModel('', false, null));
		});

		this.get('#city', function () {
			self.makeCityTabActive(new CityViewModel('', false, null));
		});

		this.get('#ip/:ip', function () {
			var ip = this.params['ip'];
			var ivm = new IpViewModel(ip, false, null);
			self.makeIpTabActive(ivm);
			$.ajax({
				method: 'GET',
				url: "/ip/location?ip=" + encodeURIComponent(ip),
				cache: false,
				success: function (data, textStatus, jqXHR) {
					var location = new Location(data);
					ivm.location(location);
					ivm.loaded(true);
				},
				error: function (jqXHR, textStatus, errorThrown) {
					ivm.error(jqXHR.responseText);
				}
			});
		});

		this.get('#city/:city', function () {
			var city = this.params['city'];
			var cvm = new CityViewModel(city, false, null);
			self.makeCityTabActive(cvm);

			$.ajax({
				method: 'GET',
				url: "/city/locations?city=" + encodeURIComponent(city),
				cache: false,
				success: function (data, textStatus, jqXHR) {
					var locations = $.map(data, function (l) { return new Location(l); });
					cvm.locations(locations);
					cvm.loaded(true);
				},
				error: function (jqXHR, textStatus, errorThrown) {
					cvm.error(jqXHR.responseText);
				}
			});
		});

		this.get('', function () { this.app.runRoute('get', '#ip') });
	}).run();
}

ko.applyBindings(new LocationsViewModel());