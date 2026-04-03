const mongoose = require('mongoose');
const bcrypt   = require('bcryptjs');

const userSchema = new mongoose.Schema({
    username:  { type: String, required: true, unique: true, trim: true, minlength: 3, maxlength: 30 },
    password:  { type: String, required: true },
    coins:     { type: String, default: '10000000' },   // stored as string for 5B+ support
    gems:      { type: Number, default: 5 },
    level:     { type: Number, default: 1 },
    xp:        { type: String, default: '0' },
    totalSpins:{ type: Number, default: 0 },
    profileImageUrl: { type: String, default: '' },     // URL to user's profile image
    createdAt: { type: Date, default: Date.now },
    lastLogin: { type: Date, default: Date.now }
}, { versionKey: false });

userSchema.pre('save', async function (next) {
    if (!this.isModified('password')) return next();
    this.password = await bcrypt.hash(this.password, 10);
    next();
});

userSchema.methods.comparePassword = function (plain) {
    return bcrypt.compare(plain, this.password);
};

userSchema.methods.toPublicJSON = function () {
    return {
        id:              this._id,
        username:        this.username,
        coins:           this.coins,
        gems:            this.gems,
        level:           this.level,
        xp:              this.xp,
        totalSpins:      this.totalSpins,
        profileImageUrl: this.profileImageUrl || ''
    };
};

module.exports = mongoose.model('User', userSchema);
