// create the ember application
App = Ember.Application.create();

// Define Routes
App.Router.map(function () {
    this.resource('sessions', function () {
        // Weird syntax alert...  Why the colon before the field name?
        // Also - 'SessionID' is CASE SENSITIVE.
        this.resource('session', { path: ':SessionID' })
    });
    this.resource('about');
    this.resource('students');
});

// To link a model to a template, you need a route.
// This creates the route:
App.SessionsRoute = Ember.Route.extend({
    model: function () {
        return this.store.find('session');
    }
});

App.SessionRoute = Ember.Route.extend({
    model: function (params) {
        return this.store.find('session', params.id);
    }
});

App.SessionController = Ember.ObjectController.extend({
    isEditing: false,
    isAdding: false,

    actions: {
        edit: function () {
            this.set('isEditing', true);
        },
        doneEditing: function (s) {
            this.set('isEditing', false);
            this.model.save();
        },
        delete: function ()
        {
            var self = this;
            this.get('model').destroyRecord().then(function () {
                self.transitionToRoute('sessions');
            });
        },
        add: function () {
            this.set('isAdding', true);
        },
        doneAdding: function (s) {
            this.set('isAdding', false);
            this.model.save();
        }
    }
});

var attr = DS.attr;

App.Session = DS.Model.extend({
    session_name: attr(),
    instructor_name: attr(),
    capacity: attr(),
    location: attr(),
    description: attr()
});

// Set the root path for the webApi service.
DS.RESTAdapter.reopen({
    namespace: 'api'
});